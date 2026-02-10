using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class PatientRepository : BaseRepository<Patient>, IPatientRepository
{
    public PatientRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Patient?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.ChronicDiseases)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Patient?> GetByIdWithIncludesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.PhoneNumbers)
            .Include(p => p.ChronicDiseases)
                .ThenInclude(pcd => pcd.ChronicDisease)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Patient?> GetByIdForClinicAsync(Guid id, Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.Id == id && p.ClinicId == clinicId, cancellationToken);
    }

    public async Task<PagedResult<Patient>> GetPagedForClinicAsync(Guid clinicId, SearchablePaginationRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking()
            .Include(p => p.PhoneNumbers)
            .Include(p => p.ChronicDiseases)
                .ThenInclude(pcd => pcd.ChronicDisease)
            .Where(p => p.ClinicId == clinicId);

        // Apply search and filtering
        query = ApplySearchAndFilters(query, request);

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = ApplySorting(query, request);

        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Patient>(items, totalCount, request.PageNumber, request.PageSize);
    }

    public async Task<int> GetCountForClinicByYearAsync(Guid clinicId, int year, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(cp => cp.ClinicId == clinicId && cp.CreatedAt.Year == year)
            .CountAsync(cancellationToken);
    }

    public async Task<PagedResult<Patient>> GetByClinicBranchIdPagedAsync(
        Guid clinicBranchId,
        string? searchTerm,
        int pageNumber,
        int pageSize,
        string? sortBy = null,
        string sortDirection = "desc",
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking()
            .Where(p => p.ClinicId == clinicBranchId);

        // Apply search
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p =>
                p.FullName.Contains(searchTerm) ||
                p.PatientCode.Contains(searchTerm) ||
                p.PhoneNumbers.Any(pn => pn.PhoneNumber.Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting using switch expression
        var isAscending = sortDirection.ToLower() != "desc";
        query = sortBy?.ToLower() switch
        {
            "fullname" or "name" => isAscending 
                ? query.OrderBy(p => p.FullName) 
                : query.OrderByDescending(p => p.FullName),
            "patientcode" => isAscending 
                ? query.OrderBy(p => p.PatientCode) 
                : query.OrderByDescending(p => p.PatientCode),
            "createdat" => isAscending 
                ? query.OrderBy(p => p.CreatedAt) 
                : query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt) // Default sort
        };

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Patient>(items, totalCount, pageNumber, pageSize);
    }

    protected override IQueryable<Patient> ApplySearchAndFilters(IQueryable<Patient> query, SearchablePaginationRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(p =>
                p.FullName.Contains(request.SearchTerm) ||
                p.PatientCode.Contains(request.SearchTerm) ||
                p.PhoneNumbers.Any(pn => pn.PhoneNumber.Contains(request.SearchTerm)));
        }

        // Apply filters from dictionary
        if (request.Filters != null && request.Filters.Count > 0)
        {
            if (request.Filters.TryGetValue("gender", out var genderObj) && genderObj is Gender gender)
            {
                query = query.Where(p => p.Gender == gender);
            }

            if (request.Filters.TryGetValue("dateOfBirthFrom", out var dobFromObj) && dobFromObj is DateTime dobFrom)
            {
                query = query.Where(p => p.DateOfBirth >= dobFrom);
            }

            if (request.Filters.TryGetValue("dateOfBirthTo", out var dobToObj) && dobToObj is DateTime dobTo)
            {
                query = query.Where(p => p.DateOfBirth <= dobTo);
            }

            if (request.Filters.TryGetValue("createdFrom", out var createdFromObj) && createdFromObj is DateTime createdFrom)
            {
                query = query.Where(p => p.CreatedAt >= createdFrom);
            }

            if (request.Filters.TryGetValue("createdTo", out var createdToObj) && createdToObj is DateTime createdTo)
            {
                query = query.Where(p => p.CreatedAt <= createdTo);
            }

            // Age filtering
            if (request.Filters.TryGetValue("minAge", out var minAgeObj) && minAgeObj is int minAge)
            {
                var maxDateOfBirth = DateTime.UtcNow.AddYears(-minAge);
                query = query.Where(p => p.DateOfBirth <= maxDateOfBirth);
            }

            if (request.Filters.TryGetValue("maxAge", out var maxAgeObj) && maxAgeObj is int maxAge)
            {
                var minDateOfBirth = DateTime.UtcNow.AddYears(-maxAge - 1);
                query = query.Where(p => p.DateOfBirth >= minDateOfBirth);
            }
        }

        return query;
    }

    protected override IQueryable<Patient> ApplySorting(IQueryable<Patient> query, SearchablePaginationRequest request)
    {
        return request.SortBy?.ToLower() switch
        {
            "fullname" or "name" => request.IsAscending 
                ? query.OrderBy(p => p.FullName) 
                : query.OrderByDescending(p => p.FullName),
            "patientcode" => request.IsAscending 
                ? query.OrderBy(p => p.PatientCode) 
                : query.OrderByDescending(p => p.PatientCode),
            "createdat" => request.IsAscending 
                ? query.OrderBy(p => p.CreatedAt) 
                : query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };
    }
}
