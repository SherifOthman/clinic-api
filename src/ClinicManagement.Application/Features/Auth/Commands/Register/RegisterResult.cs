

namespace ClinicManagement.Application.Features.Auth.Commands.Register;

public record RegisterResult(
    bool Success,
    Guid? UserId,
    string? ErrorCode,
    string? ErrorMessage
);
