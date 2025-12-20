using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Options;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace ClinicManagement.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly SmtpOptions _options;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        UserManager<User> userManager,
        IEmailSender emailSender,
        IOptions<SmtpOptions> options,
        ILogger<IdentityService> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<(bool IsSuccess, IEnumerable<ErrorItem>? Errors)> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating user account for email: {Email}, ClinicId: {ClinicId}", user.Email, user.ClinicId);
        
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed to create user {Email}. Errors: {Errors}", 
                user.Email, 
                string.Join(", ", result.Errors.Select(e => e.Description)));
            
            return (IsSuccess: false,
                  Errors:result.Errors.Select(e => 
                  new ErrorItem {
                      Field= e.Code,
                      Message = e.Description}));
        }
        
        _logger.LogInformation("Successfully created user account for {Email} with UserId: {UserId}", user.Email, user.Id);
        return (IsSuccess: true, Errors: null);
    }

    public async Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default)
    {
        var isValid = await _userManager.CheckPasswordAsync(user, password);
        
        if (!isValid)
        {
            _logger.LogWarning("Failed password check for user {Email} (UserId: {UserId})", user.Email, user.Id);
        }
        else
        {
            _logger.LogInformation("Successful password check for user {Email} (UserId: {UserId})", user.Email, user.Id);
        }
        
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
            subject: "Confirm Your Email Address",
            htmlMessage: $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #2563eb;'>Welcome to Clinic Management System!</h2>
                    <p>Hello {user.FirstName},</p>
                    <p>Thank you for registering with us. To complete your registration and start using our platform, please confirm your email address by clicking the button below:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{confirmationLink}' style='background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Confirm Email Address</a>
                    </div>
                    <p>Or copy and paste this link into your browser:</p>
                    <p style='word-break: break-all; color: #6b7280;'>{confirmationLink}</p>
                    <p style='margin-top: 30px; color: #6b7280; font-size: 14px;'>If you didn't create an account with us, please ignore this email.</p>
                    <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 30px 0;'>
                    <p style='color: #9ca3af; font-size: 12px;'>This is an automated message, please do not reply to this email.</p>
                </div>
            ",
            cancellationToken: cancellationToken);
    }

    public async Task<(bool IsSuccess, string Message)> ConfirmEmailAsync(User user, string token, CancellationToken cancellationToken = default)
    {
      var result = await  _userManager.ConfirmEmailAsync(user, token);

        if (result.Succeeded)
        {
            _logger.LogInformation("Email confirmed successfully for user {Email} (UserId: {UserId})", user.Email, user.Id);
            return (true, "Email Confirmed successfully!");
        }

        _logger.LogWarning("Failed email confirmation for user {Email} (UserId: {UserId})", user.Email, user.Id);
        return (false, "Invalid or expired token");
    }

    public async Task SendPasswordResetEmailAsync(User user, CancellationToken cancellationToken = default)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = $"{_options.FrontendUrl}/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";
        
        await _emailSender.SendEmailAsync(
            toEmail: user.Email!,
            subject: "Reset Your Password - ClinicFlow",
            htmlMessage: $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                        .content {{ background: #f9fafb; padding: 30px; border-radius: 0 0 10px 10px; }}
                        .button {{ display: inline-block; padding: 14px 32px; background: #667eea; color: white; text-decoration: none; border-radius: 8px; font-weight: bold; margin: 20px 0; }}
                        .button:hover {{ background: #5568d3; }}
                        .footer {{ text-align: center; margin-top: 20px; color: #6b7280; font-size: 14px; }}
                        .warning {{ background: #fef3c7; border-left: 4px solid #f59e0b; padding: 12px; margin: 20px 0; border-radius: 4px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1 style='margin: 0;'>🔐 Password Reset Request</h1>
                        </div>
                        <div class='content'>
                            <p>Hello <strong>{user.FirstName}</strong>,</p>
                            <p>We received a request to reset your password for your ClinicFlow account. Click the button below to create a new password:</p>
                            <div style='text-align: center;'>
                                <a href='{resetLink}' class='button'>Reset My Password</a>
                            </div>
                            <div class='warning'>
                                <strong>⏰ Important:</strong> This link will expire in <strong>1 hour</strong> for security reasons.
                            </div>
                            <p>If the button doesn't work, copy and paste this link into your browser:</p>
                            <p style='word-break: break-all; color: #667eea;'>{resetLink}</p>
                            <hr style='border: none; border-top: 1px solid #e5e7eb; margin: 30px 0;'>
                            <p style='color: #6b7280; font-size: 14px;'>If you didn't request this password reset, please ignore this email. Your password will remain unchanged.</p>
                        </div>
                        <div class='footer'>
                            <p>© 2025 ClinicFlow. All rights reserved.</p>
                            <p>Streamline your healthcare practice with confidence.</p>
                        </div>
                    </div>
                </body>
                </html>
            ",
            cancellationToken: cancellationToken);
    }

    public async Task<(bool IsSuccess, string Message)> ResetPasswordAsync(User user, string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

        if (result.Succeeded)
            return (true, "Password has been reset successfully!");

        return (false, "Invalid or expired reset token");
    }

    public async Task<(bool IsSuccess, string Message)> ChangePasswordAsync(User user, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

        if (result.Succeeded)
            return (true, "Password changed successfully!");

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return (false, errors);
    }
}
