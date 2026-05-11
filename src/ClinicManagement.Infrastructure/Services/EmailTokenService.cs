using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Services.Emails;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// OTP-based email confirmation and password reset.
///
/// Both flows:
///   1. Invalidate any existing unused OTPs for the user+type
///   2. Generate a new 6-digit OTP (stored as SHA-256 hash)
///   3. Send the OTP by email
///   4. On verify: look up by hash, check validity, mark used
///
/// Password reset additionally generates an Identity reset token on verify,
/// so UserManager.ResetPasswordAsync can be called with it.
/// </summary>
public class EmailTokenService : IEmailTokenService
{
    private static readonly TimeSpan EmailConfirmationExpiry = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan PasswordResetExpiry     = TimeSpan.FromMinutes(10);

    private readonly UserManager<User>    _userManager;
    private readonly IEmailService        _emailService;
    private readonly IUserTokenRepository _userTokens;
    private readonly IUnitOfWork          _uow;
    private readonly ILogger<EmailTokenService> _logger;

    public EmailTokenService(
        UserManager<User> userManager,
        IEmailService emailService,
        IUserTokenRepository userTokens,
        IUnitOfWork uow,
        ILogger<EmailTokenService> logger)
    {
        _userManager = userManager;
        _emailService = emailService;
        _userTokens   = userTokens;
        _uow          = uow;
        _logger       = logger;
    }

    // ── Email confirmation ────────────────────────────────────────────────────

    public async Task SendConfirmationOtpAsync(User user, CancellationToken ct = default)
    {
        // Invalidate any previous unused OTPs first
        await _userTokens.InvalidateAllAsync(user.Id, TokenTypes.EmailConfirmation, ct);

        var (entity, rawOtp) = UserToken.Create(user.Id, TokenTypes.EmailConfirmation, EmailConfirmationExpiry);
        await _userTokens.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        await _emailService.SendAsync(
            new EmailConfirmationOtpEmail(user.Email!, user.FullName, rawOtp),
            ct);

        _logger.LogInformation("Email confirmation OTP sent to {Email}", user.Email);
    }

    public async Task<bool> VerifyConfirmationOtpAsync(User user, string otp, CancellationToken ct = default)
    {
        var hash  = UserToken.Hash(otp);
        var token = await _userTokens.GetActiveByHashAsync(hash, DateTimeOffset.UtcNow, ct);

        if (token is null || token.UserId != user.Id || token.TokenType != TokenTypes.EmailConfirmation)
        {
            _logger.LogWarning("Invalid email confirmation OTP for {Email}", user.Email);
            return false;
        }

        token.MarkUsed();

        // Confirm the email via Identity
        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Email confirmed via OTP for {Email}", user.Email);
        return true;
    }

    public async Task<bool> IsEmailConfirmedAsync(User user, CancellationToken ct = default)
        => await _userManager.IsEmailConfirmedAsync(user);

    // ── Password reset ────────────────────────────────────────────────────────

    public async Task SendPasswordResetOtpAsync(User user, CancellationToken ct = default)
    {
        await _userTokens.InvalidateAllAsync(user.Id, TokenTypes.PasswordReset, ct);

        var (entity, rawOtp) = UserToken.Create(user.Id, TokenTypes.PasswordReset, PasswordResetExpiry);
        await _userTokens.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        await _emailService.SendAsync(
            new PasswordResetOtpEmail(user.Email!, user.FullName, rawOtp),
            ct);

        _logger.LogInformation("Password reset OTP sent to {Email}", user.Email);
    }

    public async Task<string?> VerifyPasswordResetOtpAsync(User user, string otp, CancellationToken ct = default)
    {
        var hash  = UserToken.Hash(otp);
        var token = await _userTokens.GetActiveByHashAsync(hash, DateTimeOffset.UtcNow, ct);

        if (token is null || token.UserId != user.Id || token.TokenType != TokenTypes.PasswordReset)
        {
            _logger.LogWarning("Invalid password reset OTP for {Email}", user.Email);
            return null;
        }

        token.MarkUsed();
        await _uow.SaveChangesAsync(ct);

        // Generate the Identity reset token now that the OTP is verified
        var identityToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        _logger.LogInformation("Password reset OTP verified for {Email}", user.Email);
        return identityToken;
    }
}
