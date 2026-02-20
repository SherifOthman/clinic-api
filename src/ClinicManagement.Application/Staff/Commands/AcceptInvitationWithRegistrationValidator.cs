using FluentValidation;
using ClinicManagement.Application.Common.Validators;

namespace ClinicManagement.Application.Staff.Commands;

public class AcceptInvitationWithRegistrationValidator : AbstractValidator<AcceptInvitationWithRegistrationCommand>
{
    public AcceptInvitationWithRegistrationValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.UserName)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MustBeValidPhoneNumber();
    }
}
