using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Common.Models;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Data.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    private readonly UserManager<User> _userManager;

    public UserRepository(ApplicationDbContext context, ICurrentUserService currentUserService, UserManager<User> userManager) : base(context)
    {
        _userManager = userManager;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        return await _userManager.GetUsersInRoleAsync(role);
    }

    public override async Task<PagedResult<User>> GetPagedAsync(PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        // Apply filters if request is UserSearchRequest
        if (request is UserSearchRequest userRequest)
        {
            // Apply search term
            if (userRequest.HasSearchTerm)
            {
                var lowerSearchTerm = userRequest.SearchTerm!.ToLower();
                query = query.Where(u => 
                    u.FirstName.ToLower().Contains(lowerSearchTerm) ||
                    u.LastName.ToLower().Contains(lowerSearchTerm) ||
                    u.Email!.ToLower().Contains(lowerSearchTerm) ||
                    (!string.IsNullOrEmpty(u.PhoneNumber) && u.PhoneNumber.Contains(userRequest.SearchTerm!)));
            }

            // Apply email confirmed filter
            if (userRequest.HasEmailConfirmedFilter)
            {
                query = query.Where(u => u.EmailConfirmed == userRequest.EmailConfirmed!.Value);
            }

            // Apply date range filters
            if (userRequest.HasCreatedDateFilter)
            {
                if (userRequest.CreatedFrom.HasValue)
                {
                    query = query.Where(u => u.CreatedAt >= userRequest.CreatedFrom.Value);
                }
                if (userRequest.CreatedTo.HasValue)
                {
                    query = query.Where(u => u.CreatedAt <= userRequest.CreatedTo.Value);
                }
            }

            // Apply sorting
            query = userRequest.SortBy?.ToLower() switch
            {
                "firstname" => userRequest.SortDescending 
                    ? query.OrderByDescending(u => u.FirstName) 
                    : query.OrderBy(u => u.FirstName),
                "lastname" => userRequest.SortDescending 
                    ? query.OrderByDescending(u => u.LastName) 
                    : query.OrderBy(u => u.LastName),
                "email" => userRequest.SortDescending 
                    ? query.OrderByDescending(u => u.Email) 
                    : query.OrderBy(u => u.Email),
                "createdat" => userRequest.SortDescending 
                    ? query.OrderByDescending(u => u.CreatedAt) 
                    : query.OrderBy(u => u.CreatedAt),
                _ => query.OrderBy(u => u.Id) // Default sort
            };
        }
        else
        {
            // Default sorting for basic pagination
            query = query.OrderBy(u => u.Id);
        }

        // Use extension method for pagination
        return await query.ToPaginatedResultAsync(request, cancellationToken);
    }
}

