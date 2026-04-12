using FluentValidation;
using ClinicManagement.Application.Common.Validators;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class AcceptInvitationWithRegistrationValidator : AbstractValidator<AcceptInvitationWithRegistrationCommand>
{
    public AcceptInvitationWithRegistrationValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[\u0600-\u06FFa-zA-Z\s'\-\.]+$")
            .WithMessage("First name must contain only letters");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100)
            .Matches(@"^[\u0600-\u06FFa-zA-Z\s'\-\.]+$")
            .WithMessage("Last name must contain only letters");

        RuleFor(x => x.UserName)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50)
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Username can only contain letters, numbers, and underscores");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#^()_+\-=\[\]{};':""\\|,.<>\/])")
            .WithMessage("Password must contain uppercase, lowercase, number, and special character");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MustBeValidPhoneNumber();
    }
}
