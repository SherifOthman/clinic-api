using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.UploadProfileImage;

public class UploadProfileImageCommandValidator : AbstractValidator<UploadProfileImageCommand>
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5MB

    public UploadProfileImageCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("Profile image file is required");

        RuleFor(x => x.File.Length)
            .LessThanOrEqualTo(MaxFileSizeInBytes)
            .WithMessage($"File size must not exceed {MaxFileSizeInBytes / 1024 / 1024}MB")
            .When(x => x.File != null);

        RuleFor(x => x.File.FileName)
            .Must(fileName => AllowedExtensions.Any(ext => fileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            .WithMessage($"Only image files are allowed: {string.Join(", ", AllowedExtensions)}")
            .When(x => x.File != null);
    }
}
