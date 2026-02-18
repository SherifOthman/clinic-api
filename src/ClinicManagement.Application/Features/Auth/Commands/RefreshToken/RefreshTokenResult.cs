

namespace ClinicManagement.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenResult(
    bool Success,
    string? AccessToken,
    string? RefreshToken,
    string? ErrorCode,
    string? ErrorMessage
);
