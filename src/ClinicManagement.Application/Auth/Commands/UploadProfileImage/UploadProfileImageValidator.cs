using FluentValidation;

namespace ClinicManagement.Application.Auth.Commands;

public class UploadProfileImageValidator : AbstractValidator<UploadProfileImageCommand>
{
    public UploadProfileImageValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required");

        RuleFor(x => x.File.Length)
            .LessThanOrEqualTo(5 * 1024 * 1024) // 5MB
            .When(x => x.File != null)
            .WithMessage("File size must not exceed 5MB");

        RuleFor(x => x.File.ContentType)
            .Must(contentType => contentType == "image/jpeg" || 
                                contentType == "image/png" || 
                                contentType == "image/jpg")
            .When(x => x.File != null)
            .WithMessage("File must be a JPEG or PNG image");
    }
}
