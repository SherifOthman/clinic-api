using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Models;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatients;

public record GetPatientsQuery(
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 10,
    string? SortBy = null,
    string SortDirection = "desc",
    Gender? Gender = null,
    DateTime? DateOfBirthFrom = null,
    DateTime? DateOfBirthTo = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    int? MinAge = null,
    int? MaxAge = null
) : IRequest<Result<PagedResult<PatientDto>>>;

public class GetPatientsQueryHandler : IRequestHandler<GetPatientsQuery, Result<PagedResult<PatientDto>>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetPatientsQueryHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<PatientDto>>> Handle(GetPatientsQuery request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUserService.ClinicId!.Value;

        var query = _context.Patients
            .Include(p => p.PhoneNumbers)
            .Include(p => p.ChronicDiseases)
                .ThenInclude(pcd => pcd.ChronicDisease)
            .Where(p => p.ClinicId == clinicId)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(p =>
                p.FullName.ToLower().Contains(searchTerm) ||
                p.PatientCode.ToLower().Contains(searchTerm) ||
                p.PhoneNumbers.Any(ph => ph.PhoneNumber.Contains(searchTerm)));
        }

        // Apply filters
        if (request.Gender.HasValue)
        {
            query = query.Where(p => p.Gender == request.Gender.Value);
        }

        if (request.DateOfBirthFrom.HasValue)
        {
            query = query.Where(p => p.DateOfBirth >= request.DateOfBirthFrom.Value);
        }

        if (request.DateOfBirthTo.HasValue)
        {
            query = query.Where(p => p.DateOfBirth <= request.DateOfBirthTo.Value);
        }

        if (request.CreatedFrom.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= request.CreatedFrom.Value);
        }

        if (request.CreatedTo.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= request.CreatedTo.Value);
        }

        // Age filtering (calculated on the fly)
        if (request.MinAge.HasValue || request.MaxAge.HasValue)
        {
            var today = DateTime.UtcNow;
            
            if (request.MaxAge.HasValue)
            {
                var minDateOfBirth = today.AddYears(-request.MaxAge.Value - 1);
                query = query.Where(p => p.DateOfBirth >= minDateOfBirth);
            }

            if (request.MinAge.HasValue)
            {
                var maxDateOfBirth = today.AddYears(-request.MinAge.Value);
                query = query.Where(p => p.DateOfBirth <= maxDateOfBirth);
            }
        }

        // Apply sorting
        var isDescending = request.SortDirection.ToLower() == "desc";
        query = ApplySorting(query, request.SortBy, isDescending);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var patients = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = patients.Adapt<List<PatientDto>>();
        var pagedResult = new PagedResult<PatientDto>(
            dtos,
            totalCount,
            request.PageNumber,
            request.PageSize);

        return Result<PagedResult<PatientDto>>.Ok(pagedResult);
    }

    private static IQueryable<Domain.Entities.Patient> ApplySorting(
        IQueryable<Domain.Entities.Patient> query,
        string? sortBy,
        bool sortDescending)
    {
        return sortBy?.ToLower() switch
        {
            "fullname" => sortDescending
                ? query.OrderByDescending(p => p.FullName)
                : query.OrderBy(p => p.FullName),
            "patientcode" => sortDescending
                ? query.OrderByDescending(p => p.PatientCode)
                : query.OrderBy(p => p.PatientCode),
            "dateofbirth" => sortDescending
                ? query.OrderByDescending(p => p.DateOfBirth)
                : query.OrderBy(p => p.DateOfBirth),
            "gender" => sortDescending
                ? query.OrderByDescending(p => p.Gender)
                : query.OrderBy(p => p.Gender),
            "createdat" => sortDescending
                ? query.OrderByDescending(p => p.CreatedAt)
                : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt) // Default sort
        };
    }
}
