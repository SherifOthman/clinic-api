using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Queries;

public class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, Result<PaginatedPatientsResponse>>
{
    private readonly IApplicationDbContext _context;

    private static readonly Dictionary<BloodType, string> BloodTypeDisplay = new()
    {
        { BloodType.APositive,  "A+"  }, { BloodType.ANegative,  "A-"  },
        { BloodType.BPositive,  "B+"  }, { BloodType.BNegative,  "B-"  },
        { BloodType.ABPositive, "AB+" }, { BloodType.ABNegative, "AB-" },
        { BloodType.OPositive,  "O+"  }, { BloodType.ONegative,  "O-"  },
    };

    public GetPatientsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<PaginatedPatientsResponse>> Handle(
        GetPatientsQuery request, CancellationToken cancellationToken)
    {
        // No Include — lightweight list query, counts only
        var query = _context.Patients.AsQueryable();
        var now = DateTime.UtcNow;

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

        var patients = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new PatientDto
            {
                Id = p.Id.ToString(),
                PatientCode = p.PatientCode,
                FullName = p.FullName,
                DateOfBirth = p.DateOfBirth.ToString("yyyy-MM-dd"),
                IsMale = p.IsMale,
                Age = p.GetAge(now),
                BloodType = p.BloodType.HasValue ? BloodTypeDisplay.GetValueOrDefault(p.BloodType.Value) : null,
                PrimaryPhone = _context.PatientPhones
                    .Where(ph => ph.PatientId == p.Id && ph.IsPrimary)
                    .Select(ph => ph.PhoneNumber)
                    .FirstOrDefault(),
                PhoneCount = _context.PatientPhones.Count(ph => ph.PatientId == p.Id && !ph.IsDeleted),
                ChronicDiseaseCount = _context.PatientChronicDiseases.Count(cd => cd.PatientId == p.Id && !cd.IsDeleted),
                CreatedAt = p.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            })
            .ToListAsync(cancellationToken);

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
