using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Templates;
using ClinicManagement.Application.Options;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;


namespace ClinicManagement.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly SmtpOptions _options;

    public IdentityService(
        UserManager<User> userManager,
        IEmailSender emailSender,
        IOptions<SmtpOptions> options)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _options = options.Value;
    }

    public async Task<Result> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.CreateAsync(user, password);
        return MapIdentityResult(result, "User created successfully");
    }

    public async Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        var isValid = await _userManager.CheckPasswordAsync(user, password);
        return isValid;
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return _userManager.FindByNameAsync(username);
    }

    public async Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _userManager.FindByIdAsync(id.ToString());
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(User user, CancellationToken cancellationToken = default)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<Result> SetUserRoleAsync(User user, string role, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.AddToRoleAsync(user, role);
        return MapIdentityResult(result, $"Role '{role}' assigned successfully");
    }

    public async Task SendConfirmationEmailAsync(User user, CancellationToken cancellationToken = default)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = $"{_options.FrontendUrl}/confirm-email?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";
        
        await _emailSender.SendEmailAsync(
            toEmail: user.Email!,
            subject: "Confirm Your Email Address",
            htmlMessage: EmailTemplates.EmailConfirmation(user.FirstName, confirmationLink),
            cancellationToken: cancellationToken);
    }

    public async Task<Result> ConfirmEmailAsync(User user, string token, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _userManager.ConfirmEmailAsync(user, token);
            return MapIdentityResult(result, "Email confirmed successfully!");
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
        {
            // Handle concurrency exception by getting fresh user data
            var freshUser = await _userManager.FindByEmailAsync(user.Email!);
            if (freshUser == null)
            {
                return Result.FailField("email", "User not found");
            }

            try
            {
                var retryResult = await _userManager.ConfirmEmailAsync(freshUser, token);
                return MapIdentityResult(retryResult, "Email confirmed successfully!");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
            {
                return Result.Fail("Email confirmation failed due to a conflict. Please try again.");
            }
        }
    }

    public async Task SendPasswordResetEmailAsync(User user, CancellationToken cancellationToken = default)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"{_options.FrontendUrl}/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";
        
        await _emailSender.SendEmailAsync(
            toEmail: user.Email!,
            subject: "Reset Your Password - ClinicFlow",
            htmlMessage: EmailTemplates.PasswordReset(user.FirstName, resetLink),
            cancellationToken: cancellationToken);
    }

    public async Task<Result> ResetPasswordAsync(User user, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return MapIdentityResult(result, "Password has been reset successfully!");
    }

    public async Task<Result> ChangePasswordAsync(User user, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return MapIdentityResult(result, "Password changed successfully!");
    }

    public async Task<bool> IsEmailConfirmedAsync(User user, CancellationToken cancellationToken = default)
    {
        return await _userManager.IsEmailConfirmedAsync(user);
    }

    /// <summary>
    /// Maps IdentityResult to application's Result pattern with proper field mapping
    /// </summary>
    private static Result MapIdentityResult(IdentityResult identityResult, string? successMessage = null)
    {
        if (identityResult.Succeeded)
            return Result.Ok(successMessage);

        var errors = identityResult.Errors.Select(e => new ErrorItem
        {
            Field = MapIdentityErrorToField(e.Code),
            Message = e.Description
        });

        return Result.Fail(errors);
    }

    /// <summary>
    /// Maps Identity error codes to meaningful field names for frontend validation
    /// </summary>
    private static string MapIdentityErrorToField(string errorCode)
    {
        return errorCode switch
        {
            "DuplicateEmail" => "email",
            "DuplicateUserName" => "username",
            "InvalidEmail" => "email",
            "PasswordTooShort" => "password",
            "PasswordRequiresDigit" => "password",
            "PasswordRequiresLower" => "password", 
            "PasswordRequiresUpper" => "password",
            "PasswordRequiresNonAlphanumeric" => "password",
            "PasswordRequiresUniqueChars" => "password",
            "InvalidToken" => "token",
            "InvalidUserName" => "username",
            "UserAlreadyHasPassword" => "password",
            "UserLockoutNotEnabled" => "general",
            "UserAlreadyInRole" => "role",
            "UserNotInRole" => "role",
            "RoleNotFound" => "role",
            "DefaultError" => "general",
            _ => "general"
        };
    }
}
