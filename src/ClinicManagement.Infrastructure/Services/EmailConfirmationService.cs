using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Templates;
using ClinicManagement.Application.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

public class EmailConfirmationService : IEmailConfirmationService
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly SmtpOptions _options;

    public EmailConfirmationService(
        UserManager<User> userManager,
        IEmailSender emailSender,
        IOptions<SmtpOptions> options)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _options = options.Value;
    }

    public async Task<Result> SendConfirmationEmailAsync(User user, CancellationToken cancellationToken = default)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = $"{_options.FrontendUrl}/confirm-email?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";
        
        var emailBody = EmailTemplates.GetEmailConfirmationTemplate($"{user.FirstName} {user.LastName}".Trim(), confirmationLink);
        
        await _emailSender.SendEmailAsync(
            user.Email!,
            "Confirm your email address",
            emailBody,
            cancellationToken);

        return Result.Ok();
    }

    public async Task<Result> ConfirmEmailAsync(User user, string token, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.ConfirmEmailAsync(user, token);
        return MapIdentityResult(result);
    }

    public async Task<bool> IsEmailConfirmedAsync(User user, CancellationToken cancellationToken = default)
    {
        return await _userManager.IsEmailConfirmedAsync(user);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    private static Result MapIdentityResult(IdentityResult identityResult)
    {
        if (identityResult.Succeeded)
            return Result.Ok();

        var errors = identityResult.Errors.Select(e => new ErrorItem(
            field: GetFieldNameFromErrorCode(e.Code),
            code: e.Description
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
