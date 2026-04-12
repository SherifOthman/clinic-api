using ClinicManagement.Application.Common.Validators;
using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.UpdateProfile;

public class UpdateProfileValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(50).WithMessage("First name must not exceed 50 characters")
            .Matches(@"^[\u0600-\u06FFa-zA-Z\s'\-\.]+$")
            .WithMessage("First name must contain only letters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(50).WithMessage("Last name must not exceed 50 characters")
            .Matches(@"^[\u0600-\u06FFa-zA-Z\s'\-\.]+$")
            .WithMessage("Last name must contain only letters");

        RuleFor(x => x.userName)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Username can only contain letters, numbers, and underscores");

        RuleFor(x => x.PhoneNumber)
            .MustBeValidPhoneNumber()
            .WithMessage("Invalid phone number format")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}
