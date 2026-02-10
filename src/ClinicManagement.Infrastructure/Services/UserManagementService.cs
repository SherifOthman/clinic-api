using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Infrastructure.Common.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        UserManager<User> userManager,
        ILogger<UserManagementService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<Result> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.CreateAsync(user, password);
        return result.ToResult();
    }

    public async Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByNameAsync(username);
    }

    public async Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByIdAsync(userId.ToString());
    }

    public async Task<IList<string>> GetUserRolesAsync(User user, CancellationToken cancellationToken = default)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<Result> AddToRoleAsync(User user, string role, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adding user {UserId} ({Email}) to role {Role}", user.Id, user.Email, role);
        var result = await _userManager.AddToRoleAsync(user, role);
        
        if (result.Succeeded)
        {
            _logger.LogInformation("Successfully added user {UserId} to role {Role}", user.Id, role);
        }
        else
        {
            _logger.LogError("Failed to add user {UserId} to role {Role}. Errors: {Errors}", 
                user.Id, role, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        
        return result.ToResult();
    }

    public async Task<string> GeneratePasswordResetTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<Result> ResetPasswordAsync(User user, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return result.ToResult();
    }
}
