using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class ClinicPatientRepository : BaseRepository<ClinicPatient>, IClinicPatientRepository
{
    public ClinicPatientRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ClinicPatient>> GetByClinicIdAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(cp => cp.Patient)
            .ThenInclude(p => p.User)
            .Where(cp => cp.ClinicId == clinicId)
            .OrderBy(cp => cp.Patient.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<ClinicPatient>> GetByClinicIdPagedAsync(Guid clinicId, SearchablePaginationRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(cp => cp.Patient)
            .ThenInclude(p => p.User)
            .Where(cp => cp.ClinicId == clinicId);

        // Apply search
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(cp => 
                cp.Patient.FullName.Contains(request.SearchTerm) ||
                (cp.Patient.User != null && cp.Patient.User.Email.Contains(request.SearchTerm)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "fullname" => request.IsAscending ? query.OrderBy(cp => cp.Patient.FullName) : query.OrderByDescending(cp => cp.Patient.FullName),
            "email" => request.IsAscending ? query.OrderBy(cp => cp.Patient.User!.Email) : query.OrderByDescending(cp => cp.Patient.User!.Email),
            "createdat" => request.IsAscending ? query.OrderBy(cp => cp.CreatedAt) : query.OrderByDescending(cp => cp.CreatedAt),
            _ => query.OrderBy(cp => cp.Patient.FullName)
        };

        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ClinicPatient>(items, totalCount, request.PageNumber, request.PageSize);
    }

    public async Task<ClinicPatient?> GetByIdAndClinicIdAsync(Guid id, Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(cp => cp.Patient)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(cp => cp.Id == id && cp.ClinicId == clinicId, cancellationToken);
    }

    public async Task<ClinicPatient?> GetByPatientIdAndClinicIdAsync(Guid patientId, Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(cp => cp.Patient)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(cp => cp.PatientId == patientId && cp.ClinicId == clinicId, cancellationToken);
    }

    protected override IQueryable<ClinicPatient> ApplySearchAndFilters(IQueryable<ClinicPatient> query, SearchablePaginationRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(cp => 
                cp.Patient.FullName.Contains(request.SearchTerm) ||
                (cp.Patient.User != null && cp.Patient.User.Email.Contains(request.SearchTerm)));
        }

        return query;
    }

    protected override IQueryable<ClinicPatient> ApplySorting(IQueryable<ClinicPatient> query, SearchablePaginationRequest request)
    {
        return request.SortBy?.ToLower() switch
        {
            "fullname" => request.IsAscending ? query.OrderBy(cp => cp.Patient.FullName) : query.OrderByDescending(cp => cp.Patient.FullName),
            "email" => request.IsAscending ? query.OrderBy(cp => cp.Patient.User!.Email) : query.OrderByDescending(cp => cp.Patient.User!.Email),
            "createdat" => request.IsAscending ? query.OrderBy(cp => cp.CreatedAt) : query.OrderByDescending(cp => cp.CreatedAt),
            _ => query.OrderBy(cp => cp.Patient.FullName)
        };
    }
}
