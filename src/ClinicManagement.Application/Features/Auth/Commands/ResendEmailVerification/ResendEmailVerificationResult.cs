

namespace ClinicManagement.Application.Features.Auth.Commands.ResendEmailVerification;

public record ResendEmailVerificationResult(
    bool Success,
    string? ErrorCode,
    string? ErrorMessage
);
