using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class MedicalSupplyRepository : BaseRepository<MedicalSupply>, IMedicalSupplyRepository
{
    public MedicalSupplyRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<MedicalSupply>> GetByClinicBranchIdPagedAsync(
        Guid clinicBranchId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking()
            .Where(ms => ms.ClinicBranchId == clinicBranchId);

        var totalCount = await query.CountAsync(cancellationToken);

        // Default sorting by name
        query = query.OrderBy(ms => ms.Name);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<MedicalSupply>(items, totalCount, pageNumber, pageSize);
    }

    protected override IQueryable<MedicalSupply> ApplySearchAndFilters(IQueryable<MedicalSupply> query, SearchablePaginationRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(ms => ms.Name.Contains(request.SearchTerm));
        }

        // Apply filters from dictionary
        if (request.Filters != null && request.Filters.Count > 0)
        {
            if (request.Filters.TryGetValue("clinicBranchId", out var branchIdObj) && branchIdObj is Guid branchId)
            {
                query = query.Where(ms => ms.ClinicBranchId == branchId);
            }

            if (request.Filters.TryGetValue("isLowStock", out var isLowStockObj) && isLowStockObj is bool isLowStock && isLowStock)
            {
                query = query.Where(ms => ms.QuantityInStock <= ms.MinimumStockLevel);
            }
        }

        return query;
    }

    protected override IQueryable<MedicalSupply> ApplySorting(IQueryable<MedicalSupply> query, SearchablePaginationRequest request)
    {
        return request.SortBy?.ToLower() switch
        {
            "name" => request.IsAscending 
                ? query.OrderBy(ms => ms.Name) 
                : query.OrderByDescending(ms => ms.Name),
            "quantityinstock" or "stock" => request.IsAscending 
                ? query.OrderBy(ms => ms.QuantityInStock) 
                : query.OrderByDescending(ms => ms.QuantityInStock),
            "unitprice" or "price" => request.IsAscending 
                ? query.OrderBy(ms => ms.UnitPrice) 
                : query.OrderByDescending(ms => ms.UnitPrice),
            "createdat" => request.IsAscending 
                ? query.OrderBy(ms => ms.CreatedAt) 
                : query.OrderByDescending(ms => ms.CreatedAt),
            _ => query.OrderBy(ms => ms.Name)
        };
    }
}
