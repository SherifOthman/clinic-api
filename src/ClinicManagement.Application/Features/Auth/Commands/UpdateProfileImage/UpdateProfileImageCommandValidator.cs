using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.UpdateProfileImage;

public class UpdateProfileImageCommandValidator : AbstractValidator<UpdateProfileImageCommand>
{
    public UpdateProfileImageCommandValidator()
    {
        RuleFor(x => x.ProfileImageUrl)
            .NotEmpty()
            .WithMessage("Profile image URL is required")
            .MaximumLength(500)
            .WithMessage("Profile image URL cannot exceed 500 characters");
    }
}