

namespace ClinicManagement.Application.Features.Auth.Commands.Login;

public record LoginResult(
    bool Success,
    string? AccessToken,
    string? RefreshToken,
    string? ErrorCode,
    string? ErrorMessage,
    bool EmailNotConfirmed = false
);
