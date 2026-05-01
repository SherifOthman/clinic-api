using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.ConfirmEmail;

public class ConfirmEmailValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();
    

        RuleFor(x => x.Token)
            .NotEmpty();
    }
}
