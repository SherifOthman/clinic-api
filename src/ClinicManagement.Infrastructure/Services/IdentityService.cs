using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
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

    public async Task<(bool IsSuccess, IEnumerable<ErrorItem>? Errors)> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return (IsSuccess: false,
                  Errors:result.Errors.Select(e => 
                  new ErrorItem {
                      Field= e.Code,
                      Message = e.Description}));
        }
        return (IsSuccess: true, Errors: null);
    }

    public Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        return _userManager.CheckPasswordAsync(user, password);
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

    public Task SetUserRoleAsync(User user, string role, CancellationToken cancellationToken = default)
    {
      return  _userManager.AddToRoleAsync(user, role);
    }

    public async Task SendConfirmationEmailAsync(User user, CancellationToken cancellationToken = default)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink =
    $"{_options.FrontendUrl}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";
        await _emailSender.SendEmailAsync(
            toEmail: user.Email!,
            subject: "Confirm your email",
            htmlMessage: $"<p>Welcome!</p><p>Click here to confirm your email: <a href='{confirmationLink}'>Confirm</a></p>",
            cancellationToken: cancellationToken);
    }

    public async Task<(bool IsSuccess, string Message)> ConfirmEmailAsync(User user, string token, CancellationToken cancellationToken = default)
    {
      var result = await  _userManager.ConfirmEmailAsync(user, token);

        if (result.Succeeded)
            return (true, "Email Confirmed successfully!");

        return (false, "Invalid or expired token");
    }
}
