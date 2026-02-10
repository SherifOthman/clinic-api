using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class MedicineRepository : BaseRepository<Medicine>, IMedicineRepository
{
    public MedicineRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<Medicine>> GetByClinicBranchIdPagedAsync(
        Guid clinicBranchId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking()
            .Where(m => m.ClinicBranchId == clinicBranchId);

        var totalCount = await query.CountAsync(cancellationToken);

        // Default sorting by name
        query = query.OrderBy(m => m.Name);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Medicine>(items, totalCount, pageNumber, pageSize);
    }

    protected override IQueryable<Medicine> ApplySearchAndFilters(IQueryable<Medicine> query, SearchablePaginationRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(m =>
                m.Name.Contains(request.SearchTerm) ||
                (m.Manufacturer != null && m.Manufacturer.Contains(request.SearchTerm)) ||
                (m.BatchNumber != null && m.BatchNumber.Contains(request.SearchTerm)));
        }

        // Apply filters from dictionary
        if (request.Filters != null && request.Filters.Count > 0)
        {
            if (request.Filters.TryGetValue("clinicBranchId", out var branchIdObj) && branchIdObj is Guid branchId)
            {
                query = query.Where(m => m.ClinicBranchId == branchId);
            }

            if (request.Filters.TryGetValue("isActive", out var isActiveObj) && isActiveObj is bool isActive)
            {
                query = query.Where(m => m.IsActive == isActive);
            }

            if (request.Filters.TryGetValue("isLowStock", out var isLowStockObj) && isLowStockObj is bool isLowStock && isLowStock)
            {
                query = query.Where(m => m.TotalStripsInStock <= m.MinimumStockLevel);
            }

            if (request.Filters.TryGetValue("isExpired", out var isExpiredObj) && isExpiredObj is bool isExpired && isExpired)
            {
                var now = DateTime.UtcNow.Date;
                query = query.Where(m => m.ExpiryDate.HasValue && m.ExpiryDate.Value.Date < now);
            }
        }

        return query;
    }

    protected override IQueryable<Medicine> ApplySorting(IQueryable<Medicine> query, SearchablePaginationRequest request)
    {
        return request.SortBy?.ToLower() switch
        {
            "name" => request.IsAscending 
                ? query.OrderBy(m => m.Name) 
                : query.OrderByDescending(m => m.Name),
            "expirydate" => request.IsAscending 
                ? query.OrderBy(m => m.ExpiryDate) 
                : query.OrderByDescending(m => m.ExpiryDate),
            "totalstripsinstock" or "stock" => request.IsAscending 
                ? query.OrderBy(m => m.TotalStripsInStock) 
                : query.OrderByDescending(m => m.TotalStripsInStock),
            "boxprice" or "price" => request.IsAscending 
                ? query.OrderBy(m => m.BoxPrice) 
                : query.OrderByDescending(m => m.BoxPrice),
            "createdat" => request.IsAscending 
                ? query.OrderBy(m => m.CreatedAt) 
                : query.OrderByDescending(m => m.CreatedAt),
            _ => query.OrderBy(m => m.Name)
        };
    }
}
