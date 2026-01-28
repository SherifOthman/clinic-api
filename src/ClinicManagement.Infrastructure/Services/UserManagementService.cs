using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Infrastructure.Services;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<User> _userManager;

    public UserManagementService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.CreateAsync(user, password);
        return MapIdentityResult(result);
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

    public async Task<User?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByIdAsync(userId.ToString());
    }

    public async Task<IList<string>> GetUserRolesAsync(User user, CancellationToken cancellationToken = default)
    {
        return await _userManager.GetRolesAsync(user);
    }

    private static Result MapIdentityResult(IdentityResult identityResult)
    {
        if (identityResult.Succeeded)
            return Result.Ok();

        var errors = identityResult.Errors.Select(e => new ErrorItem(
            field: GetFieldNameFromErrorCode(e.Code),
            message: e.Description
        )).ToList();

        return Result.Fail(errors);
    }

    private static string GetFieldNameFromErrorCode(string errorCode) => errorCode switch
    {
        "DuplicateEmail" or "InvalidEmail" => "Email",
        "DuplicateUserName" or "InvalidUserName" => "UserName",
        "PasswordTooShort" or "PasswordRequiresDigit" or "PasswordRequiresLower" 
            or "PasswordRequiresUpper" or "PasswordRequiresNonAlphanumeric" => "Password",
        _ => string.Empty
    };
}