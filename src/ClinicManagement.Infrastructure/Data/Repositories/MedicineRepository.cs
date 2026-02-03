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

    public async Task<IEnumerable<Medicine>> GetByClinicBranchIdAsync(Guid clinicBranchId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(m => m.ClinicBranchId == clinicBranchId)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<Medicine>> GetByClinicBranchIdPagedAsync(Guid clinicBranchId, SearchablePaginationRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(m => m.ClinicBranchId == clinicBranchId);

        // Apply search
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(m => m.Name.Contains(request.SearchTerm));
        }

        // Apply filters
        if (request.Filters.ContainsKey("isLowStock") && bool.TryParse(request.Filters["isLowStock"].ToString(), out bool isLowStock))
        {
            if (isLowStock)
            {
                query = query.Where(m => m.TotalStripsInStock <= m.MinimumStockLevel);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.IsAscending ? query.OrderBy(m => m.Name) : query.OrderByDescending(m => m.Name),
            "boxprice" => request.IsAscending ? query.OrderBy(m => m.BoxPrice) : query.OrderByDescending(m => m.BoxPrice),
            "stock" => request.IsAscending ? query.OrderBy(m => m.TotalStripsInStock) : query.OrderByDescending(m => m.TotalStripsInStock),
            "createdat" => request.IsAscending ? query.OrderBy(m => m.CreatedAt) : query.OrderByDescending(m => m.CreatedAt),
            _ => query.OrderBy(m => m.Name)
        };

        var items = await query
            .Skip(request.Skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Medicine>(items, totalCount, request.PageNumber, request.PageSize);
    }

    public async Task<Medicine?> GetByIdAndClinicBranchIdAsync(Guid id, Guid clinicBranchId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == id && m.ClinicBranchId == clinicBranchId, cancellationToken);
    }

    public async Task<Medicine?> GetByNameAndClinicBranchAsync(string name, Guid clinicBranchId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Name.ToLower() == name.ToLower() && m.ClinicBranchId == clinicBranchId, cancellationToken);
    }

    public async Task<IEnumerable<Medicine>> GetLowStockMedicinesAsync(Guid clinicBranchId, int threshold = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(m => m.ClinicBranchId == clinicBranchId && m.TotalStripsInStock <= threshold)
            .ToListAsync(cancellationToken);
    }

    protected override IQueryable<Medicine> ApplySearchAndFilters(IQueryable<Medicine> query, SearchablePaginationRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(m => m.Name.Contains(request.SearchTerm));
        }

        return query;
    }

    protected override IQueryable<Medicine> ApplySorting(IQueryable<Medicine> query, SearchablePaginationRequest request)
    {
        return request.SortBy?.ToLower() switch
        {
            "name" => request.IsAscending ? query.OrderBy(m => m.Name) : query.OrderByDescending(m => m.Name),
            "boxprice" => request.IsAscending ? query.OrderBy(m => m.BoxPrice) : query.OrderByDescending(m => m.BoxPrice),
            "stock" => request.IsAscending ? query.OrderBy(m => m.TotalStripsInStock) : query.OrderByDescending(m => m.TotalStripsInStock),
            "createdat" => request.IsAscending ? query.OrderBy(m => m.CreatedAt) : query.OrderByDescending(m => m.CreatedAt),
            _ => query.OrderBy(m => m.Name)
        };
    }
}
