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

    public async Task<IEnumerable<Patient>> GetByClinicIdAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(p => p.ClinicId == clinicId)
            .OrderBy(p => $"{p.FirstName} {p.LastName}")
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Patient>> GetByClinicIdPagedAsync(Guid clinicId, SearchablePaginationRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(p => p.ClinicId == clinicId);

        // Apply search
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(p => 
                (p.FirstName + " " + p.LastName).Contains(request.SearchTerm) ||
                p.PatientCode.Contains(request.SearchTerm));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "fullname" => request.IsAscending ? query.OrderBy(p => p.FirstName).ThenBy(p => p.LastName) : query.OrderByDescending(p => p.FirstName).ThenByDescending(p => p.LastName),
            "patientcode" => request.IsAscending ? query.OrderBy(p => p.PatientCode) : query.OrderByDescending(p => p.PatientCode),
            "createdat" => request.IsAscending ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderBy(p => p.FirstName).ThenBy(p => p.LastName)
        };

        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Patient>(items, totalCount, request.PageNumber, request.PageSize);
    }

    public async Task<Patient?> GetByIdAndClinicIdAsync(Guid id, Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.ClinicId == clinicId, cancellationToken);
    }

    public async Task<Patient?> GetByPatientIdAndClinicIdAsync(Guid patientId, Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == patientId && p.ClinicId == clinicId, cancellationToken);
    }

    protected override IQueryable<Patient> ApplySearchAndFilters(IQueryable<Patient> query, SearchablePaginationRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(p => 
                (p.FirstName + " " + p.LastName).Contains(request.SearchTerm) ||
                p.PatientCode.Contains(request.SearchTerm));
        }

        return query;
    }

    protected override IQueryable<Patient> ApplySorting(IQueryable<Patient> query, SearchablePaginationRequest request)
    {
        return request.SortBy?.ToLower() switch
        {
            "fullname" => request.IsAscending ? query.OrderBy(p => p.FirstName).ThenBy(p => p.LastName) : query.OrderByDescending(p => p.FirstName).ThenByDescending(p => p.LastName),
            "patientcode" => request.IsAscending ? query.OrderBy(p => p.PatientCode) : query.OrderByDescending(p => p.PatientCode),
            "createdat" => request.IsAscending ? query.OrderBy(p => p.CreatedAt) : query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderBy(p => p.FirstName).ThenBy(p => p.LastName)
        };
    }
}
