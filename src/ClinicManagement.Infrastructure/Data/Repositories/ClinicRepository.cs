using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class ClinicRepository : BaseRepository<Clinic>, IClinicRepository
{
    public ClinicRepository(ApplicationDbContext context) : base(context)
    {
    }

    public override async Task<PagedResult<Clinic>> GetPagedAsync(PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        // Apply filters if request is ClinicSearchRequest
        if (request is ClinicSearchRequest clinicRequest)
        {
            // Apply search term
            if (clinicRequest.HasSearchTerm)
            {
                var lowerSearchTerm = clinicRequest.SearchTerm!.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(lowerSearchTerm));
            }

            // Apply subscription plan filter
            if (clinicRequest.HasSubscriptionPlanFilter)
            {
                query = query.Where(c => c.SubscriptionPlanId == clinicRequest.SubscriptionPlanId!.Value);
            }

            // Apply active status filter
            if (clinicRequest.HasActiveFilter)
            {
                query = query.Where(c => c.IsActive == clinicRequest.IsActive!.Value);
            }

            // Apply date range filters
            if (clinicRequest.HasCreatedDateFilter)
            {
                if (clinicRequest.CreatedFrom.HasValue)
                {
                    query = query.Where(c => c.CreatedAt >= clinicRequest.CreatedFrom.Value);
                }
                if (clinicRequest.CreatedTo.HasValue)
                {
                    query = query.Where(c => c.CreatedAt <= clinicRequest.CreatedTo.Value);
                }
            }

            // Apply user count filters
            if (clinicRequest.HasUserCountFilter)
            {
                if (clinicRequest.MinUsers.HasValue)
                {
                    query = query.Where(c => c.Users.Count >= clinicRequest.MinUsers.Value);
                }
                if (clinicRequest.MaxUsers.HasValue)
                {
                    query = query.Where(c => c.Users.Count <= clinicRequest.MaxUsers.Value);
                }
            }

            // Apply sorting
            query = clinicRequest.SortBy?.ToLower() switch
            {
                "name" => clinicRequest.SortDescending 
                    ? query.OrderByDescending(c => c.Name) 
                    : query.OrderBy(c => c.Name),
                "createdat" => clinicRequest.SortDescending 
                    ? query.OrderByDescending(c => c.CreatedAt) 
                    : query.OrderBy(c => c.CreatedAt),
                "subscriptionplan" => clinicRequest.SortDescending 
                    ? query.OrderByDescending(c => c.SubscriptionPlan.Name) 
                    : query.OrderBy(c => c.SubscriptionPlan.Name),
                "usercount" => clinicRequest.SortDescending 
                    ? query.OrderByDescending(c => c.Users.Count) 
                    : query.OrderBy(c => c.Users.Count),
                "isactive" => clinicRequest.SortDescending 
                    ? query.OrderByDescending(c => c.IsActive) 
                    : query.OrderBy(c => c.IsActive),
                _ => query.OrderBy(c => c.Id) // Default sort
            };
        }
        else
        {
            // Default sorting for basic pagination
            query = query.OrderBy(c => c.Id);
        }

        // Use extension method for pagination
        return await query.ToPaginatedResultAsync(request, cancellationToken);
    }
}