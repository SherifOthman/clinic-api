using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class MedicalServiceRepository : BaseRepository<MedicalService>, IMedicalServiceRepository
{
    public MedicalServiceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MedicalService>> GetByClinicBranchIdAsync(Guid clinicBranchId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(ms => ms.ClinicBranchId == clinicBranchId)
            .OrderBy(ms => ms.Name)
            .ToListAsync(cancellationToken);
    }

    protected override IQueryable<MedicalService> ApplySearchAndFilters(IQueryable<MedicalService> query, SearchablePaginationRequest request)
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

            if (request.Filters.TryGetValue("isOperation", out var isOperationObj) && isOperationObj is bool isOperation)
            {
                query = query.Where(ms => ms.IsOperation == isOperation);
            }
        }

        return query;
    }

    protected override IQueryable<MedicalService> ApplySorting(IQueryable<MedicalService> query, SearchablePaginationRequest request)
    {
        return request.SortBy?.ToLower() switch
        {
            "name" => request.IsAscending 
                ? query.OrderBy(ms => ms.Name) 
                : query.OrderByDescending(ms => ms.Name),
            "defaultprice" or "price" => request.IsAscending 
                ? query.OrderBy(ms => ms.DefaultPrice) 
                : query.OrderByDescending(ms => ms.DefaultPrice),
            "isoperation" => request.IsAscending 
                ? query.OrderBy(ms => ms.IsOperation) 
                : query.OrderByDescending(ms => ms.IsOperation),
            "createdat" => request.IsAscending 
                ? query.OrderBy(ms => ms.CreatedAt) 
                : query.OrderByDescending(ms => ms.CreatedAt),
            _ => query.OrderBy(ms => ms.Name)
        };
    }
}
