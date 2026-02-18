

namespace ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;

public record ConfirmEmailResult(
    bool Success,
    bool AlreadyConfirmed,
    string? ErrorCode,
    string? ErrorMessage
);
