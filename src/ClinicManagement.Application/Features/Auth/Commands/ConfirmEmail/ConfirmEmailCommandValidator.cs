using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;

internal class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Token)
            .NotEmpty();
    }
}
