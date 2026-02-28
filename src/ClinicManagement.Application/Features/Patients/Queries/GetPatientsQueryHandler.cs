using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Queries;

public class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, Result<PaginatedPatientsResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetPatientsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result<PaginatedPatientsResponse>> Handle(
        GetPatientsQuery request,
        CancellationToken cancellationToken)
    {
        var clinicId = _currentUser.GetRequiredClinicId();

        var query = _context.Patients
            .Include(p => p.Phones)
            .Where(p => p.ClinicId == clinicId && !p.IsDeleted);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(p =>
                p.FullName.Contains(request.SearchTerm) ||
                p.PatientCode.Contains(request.SearchTerm) ||
                p.Phones.Any(ph => ph.PhoneNumber.Contains(request.SearchTerm)));
        }

        // Apply gender filter
        if (request.IsMale.HasValue)
        {
            query = query.Where(p => p.IsMale == request.IsMale.Value);
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDirection == "desc"
                ? query.OrderByDescending(p => p.FullName)
                : query.OrderBy(p => p.FullName),
            "createdat" => request.SortDirection == "desc"
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
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
                Age = DateTime.UtcNow.Year - p.DateOfBirth.Year,
                BloodType = p.BloodType.HasValue ? p.BloodType.Value.ToString() : null,
                PhoneNumbers = p.Phones.Select(ph => ph.PhoneNumber).ToList(),
                PrimaryPhone = p.Phones.Where(ph => ph.IsPrimary).Select(ph => ph.PhoneNumber).FirstOrDefault()
                    ?? p.Phones.OrderBy(ph => ph.CreatedAt).Select(ph => ph.PhoneNumber).FirstOrDefault(),
                CreatedAt = p.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ss")
            })
            .ToListAsync(cancellationToken);

        var response = new PaginatedPatientsResponse
        {
            Items = patients,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };

        return Result<PaginatedPatientsResponse>.Success(response);
    }
}
