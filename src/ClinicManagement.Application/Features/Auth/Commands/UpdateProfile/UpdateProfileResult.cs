

namespace ClinicManagement.Application.Features.Auth.Commands.UpdateProfile;

public record UpdateProfileResult(
    bool Success,
    string? ErrorCode,
    string? ErrorMessage
);
