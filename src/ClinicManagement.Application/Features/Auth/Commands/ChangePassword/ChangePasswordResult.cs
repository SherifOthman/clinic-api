

namespace ClinicManagement.Application.Features.Auth.Commands.ChangePassword;

public record ChangePasswordResult(
    bool Success,
    string? ErrorCode,
    string? ErrorMessage
);
