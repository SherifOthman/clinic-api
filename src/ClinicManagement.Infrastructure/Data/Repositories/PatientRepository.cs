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
