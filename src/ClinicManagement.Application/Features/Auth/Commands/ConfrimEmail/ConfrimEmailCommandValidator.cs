

using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.ConfrimEmail;

internal class ConfrimEmailCommandValidator : AbstractValidator<ConfrimEmailCommand>
{
    public ConfrimEmailCommandValidator()
    {

        RuleFor(x => x.UserId)
            .NotEmpty()
            .GreaterThan(0);

        RuleFor(x => x.Token)
            .NotEmpty();
    }
}
