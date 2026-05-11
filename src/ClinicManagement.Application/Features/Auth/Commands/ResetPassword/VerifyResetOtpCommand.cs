using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.ResetPassword;

/// <summary>
/// Two-step password reset via OTP:
///   Step 1 — POST /api/auth/verify-reset-otp  → verifies OTP, returns a short-lived reset token
///   Step 2 — POST /api/auth/reset-password     → uses the reset token + new password (existing endpoint)
///
/// The reset token returned here is a standard Identity password reset token,
/// so the existing ResetPasswordCommand/Handler works unchanged.
/// </summary>
public record VerifyResetOtpCommand(string Email, string Otp) : IRequest<Result<string>>;
