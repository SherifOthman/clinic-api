using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;

/// <summary>
/// Verifies a 6-digit OTP to confirm the user's email address.
/// Works on web and mobile — no redirect links needed.
/// </summary>
public record VerifyEmailOtpCommand(string Email, string Otp) : IRequest<Result>;
