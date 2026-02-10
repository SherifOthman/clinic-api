using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;

internal class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is invalid");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Confirmation token is required");
    }
}
