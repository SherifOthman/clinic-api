

namespace ClinicManagement.Application.Features.Auth.Commands.DeleteProfileImage;

public record DeleteProfileImageResult(
    bool Success,
    string? ErrorCode,
    string? ErrorMessage
);
