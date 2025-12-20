using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;

internal class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(x => x.Token)
            .NotEmpty();
    }
}
