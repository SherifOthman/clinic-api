using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Email;

/// <summary>
/// Manages OTP-based email verification and password reset.
///
/// Both flows use a 6-digit OTP sent by email — works identically on web and mobile.
/// No redirect links, no deep links, no URL encoding issues.
/// </summary>
public interface IEmailTokenService
{
    // ── Email confirmation ────────────────────────────────────────────────────

    /// <summary>
    /// Generates a 6-digit OTP, stores its hash, and sends the confirmation email.
    /// Invalidates any previous unused OTPs for this user first.
    /// </summary>
    Task SendConfirmationOtpAsync(User user, CancellationToken ct = default);

    /// <summary>
    /// Verifies the OTP and marks the user's email as confirmed.
    /// Returns false if the OTP is invalid, expired, or already used.
    /// </summary>
    Task<bool> VerifyConfirmationOtpAsync(User user, string otp, CancellationToken ct = default);

    Task<bool> IsEmailConfirmedAsync(User user, CancellationToken ct = default);

    // ── Password reset ────────────────────────────────────────────────────────

    /// <summary>
    /// Generates a 6-digit OTP, stores its hash, and sends the password reset email.
    /// Invalidates any previous unused OTPs for this user first.
    /// </summary>
    Task SendPasswordResetOtpAsync(User user, CancellationToken ct = default);

    /// <summary>
    /// Verifies the OTP. Returns the raw Identity reset token if valid,
    /// so the caller can pass it to UserManager.ResetPasswordAsync.
    /// Returns null if the OTP is invalid, expired, or already used.
    /// </summary>
    Task<string?> VerifyPasswordResetOtpAsync(User user, string otp, CancellationToken ct = default);
}
