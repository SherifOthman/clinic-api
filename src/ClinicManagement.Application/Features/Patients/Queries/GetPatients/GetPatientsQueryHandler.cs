using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Queries;

public class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, Result<PaginatedPatientsResponse>>
{
    private readonly IApplicationDbContext _context;

    public GetPatientsQueryHandler(IApplicationDbContext context) => _context = context;

    private static string? ToDisplayString(BloodType? bt) => bt switch
    {
        BloodType.APositive  => "A+",
        BloodType.ANegative  => "A-",
        BloodType.BPositive  => "B+",
        BloodType.BNegative  => "B-",
        BloodType.ABPositive => "AB+",
        BloodType.ABNegative => "AB-",
        BloodType.OPositive  => "O+",
        BloodType.ONegative  => "O-",
        _                    => null,
    };

    public async Task<Result<PaginatedPatientsResponse>> Handle(
        GetPatientsQuery request, CancellationToken cancellationToken)
    {
        // SuperAdmin bypasses the tenant filter to see all clinics
        var query = request.IsSuperAdmin
            ? _context.Patients.IgnoreQueryFilters([QueryFilterNames.Tenant]).AsQueryable()
            : _context.Patients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm;
            var matchingPatientIds = _context.PatientPhones
                .Where(ph => ph.PhoneNumber.Contains(term))
                .Select(ph => ph.PatientId);

            query = query.Where(p =>
                p.FullName.Contains(term) ||
                p.PatientCode.Contains(term) ||
                matchingPatientIds.Contains(p.Id));
        }

        if (request.IsMale.HasValue)
            query = query.Where(p => p.IsMale == request.IsMale.Value);

        // SuperAdmin can filter by a specific clinic
        if (request.IsSuperAdmin && request.ClinicId.HasValue)
            query = query.Where(p => p.ClinicId == request.ClinicId.Value);

        query = request.SortBy?.ToLower() switch
        {
            "name"      => request.SortDirection == "desc"
                            ? query.OrderByDescending(p => p.FullName)
                            : query.OrderBy(p => p.FullName),
            "age"       => request.SortDirection == "desc"
                            ? query.OrderBy(p => p.DateOfBirth)
                            : query.OrderByDescending(p => p.DateOfBirth),
            "createdat" => request.SortDirection == "desc"
                            ? query.OrderByDescending(p => p.CreatedAt)
                            : query.OrderBy(p => p.CreatedAt),
            _           => query.OrderByDescending(p => p.CreatedAt),
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var rawPatients = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new
            {
                Id = p.Id.ToString(),
                p.PatientCode,
                p.FullName,
                p.DateOfBirth,
                p.IsMale,
                p.BloodType,
                p.ClinicId,
                PrimaryPhone = _context.PatientPhones
                    .Select(ph => ph.PhoneNumber)
                    .FirstOrDefault(),
                PhoneCount = _context.PatientPhones.Count(ph => ph.PatientId == p.Id),
                ChronicDiseaseCount = _context.PatientChronicDiseases.Count(cd => cd.PatientId == p.Id),
                p.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        // Resolve clinic names in one query for SuperAdmin
        Dictionary<Guid, string> clinicNames = new();
        if (request.IsSuperAdmin)
        {
            var clinicIds = rawPatients
                .Select(p => p.ClinicId)
                .Distinct()
                .ToList();

            if (clinicIds.Count > 0)
                clinicNames = await _context.Clinics
                    .IgnoreQueryFilters([QueryFilterNames.Tenant])
                    .Where(c => clinicIds.Contains(c.Id))
                    .Select(c => new { c.Id, c.Name })
                    .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);
        }

        var patients = rawPatients.Select(p => new PatientDto
        {
            Id = p.Id,
            PatientCode = p.PatientCode,
            FullName = p.FullName,
            DateOfBirth = p.DateOfBirth.ToString("yyyy-MM-dd"),
            IsMale = p.IsMale,
            BloodType = ToDisplayString(p.BloodType),
            PrimaryPhone = p.PrimaryPhone,
            PhoneCount = p.PhoneCount,
            ChronicDiseaseCount = p.ChronicDiseaseCount,
            CreatedAt = p.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            ClinicName = clinicNames.TryGetValue(p.ClinicId, out var clinicName) ? clinicName : null,
        }).ToList();

        return Result<PaginatedPatientsResponse>.Success(new PaginatedPatientsResponse
        {
            Items      = patients,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize   = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
        });
    }
}
