using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.UpdateProfileImage;

public class UpdateProfileImageCommandValidator : AbstractValidator<UpdateProfileImageCommand>
{
    private readonly string[] _allowedContentTypes = { 
        "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" 
    };
    private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB

    public UpdateProfileImageCommandValidator()
    {
        RuleFor(x => x.Image)
            .NotNull()
            .WithMessage("Image file is required");

        RuleFor(x => x.Image.Length)
            .LessThanOrEqualTo(_maxFileSize)
            .When(x => x.Image != null)
            .WithMessage($"Image file size must not exceed {_maxFileSize / (1024 * 1024)}MB");

        RuleFor(x => x.Image.ContentType)
            .Must(contentType => _allowedContentTypes.Contains(contentType?.ToLowerInvariant()))
            .When(x => x.Image != null)
            .WithMessage("Only image files (JPEG, PNG, GIF, WebP) are allowed");

        RuleFor(x => x.Image.FileName)
            .NotEmpty()
            .When(x => x.Image != null)
            .WithMessage("Image file name is required");
    }
}