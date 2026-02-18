

namespace ClinicManagement.Application.Features.Auth.Commands.ResetPassword;

public record ResetPasswordResult(
    bool Success,
    string? ErrorCode,
    string? ErrorMessage
);
