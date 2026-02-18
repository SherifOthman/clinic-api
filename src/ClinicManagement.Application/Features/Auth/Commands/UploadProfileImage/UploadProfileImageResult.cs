

namespace ClinicManagement.Application.Features.Auth.Commands.UploadProfileImage;

public record UploadProfileImageResult(
    bool Success,
    string? ErrorCode,
    string? ErrorMessage
);
