using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.ResendEmailVerification;

public class ResendEmailVerificationValidator : AbstractValidator<ResendEmailVerificationCommand>
{
    public ResendEmailVerificationValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format");
    }
}
