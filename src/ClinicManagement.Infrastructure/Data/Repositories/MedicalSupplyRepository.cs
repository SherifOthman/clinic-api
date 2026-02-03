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

    public async Task<IEnumerable<MedicalSupply>> GetByClinicBranchIdAsync(Guid clinicBranchId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.ClinicBranchId == clinicBranchId)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<MedicalSupply>> GetByClinicBranchIdPagedAsync(Guid clinicBranchId, SearchablePaginationRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(s => s.ClinicBranchId == clinicBranchId);

        // Apply search
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(s => s.Name.Contains(request.SearchTerm));
        }

        // Apply filters
        if (request.Filters.ContainsKey("isLowStock") && bool.TryParse(request.Filters["isLowStock"].ToString(), out bool isLowStock))
        {
            if (isLowStock)
            {
                query = query.Where(s => s.QuantityInStock <= s.MinimumStockLevel);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.IsAscending ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name),
            "unitprice" => request.IsAscending ? query.OrderBy(s => s.UnitPrice) : query.OrderByDescending(s => s.UnitPrice),
            "stock" => request.IsAscending ? query.OrderBy(s => s.QuantityInStock) : query.OrderByDescending(s => s.QuantityInStock),
            "createdat" => request.IsAscending ? query.OrderBy(s => s.CreatedAt) : query.OrderByDescending(s => s.CreatedAt),
            _ => query.OrderBy(s => s.Name)
        };

        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<MedicalSupply>(items, totalCount, request.PageNumber, request.PageSize);
    }

    public async Task<MedicalSupply?> GetByIdAndClinicBranchIdAsync(Guid id, Guid clinicBranchId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id && s.ClinicBranchId == clinicBranchId, cancellationToken);
    }

    public async Task<IEnumerable<MedicalSupply>> GetLowStockSuppliesAsync(Guid clinicBranchId, int threshold = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.ClinicBranchId == clinicBranchId && s.QuantityInStock <= threshold)
            .ToListAsync(cancellationToken);
    }
}