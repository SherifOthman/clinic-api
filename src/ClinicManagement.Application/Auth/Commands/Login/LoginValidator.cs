using FluentValidation;

namespace ClinicManagement.Application.Auth.Commands.Login;

public class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.EmailOrUsername)
            .NotEmpty()
            .WithMessage("Email or username is required")
            .MinimumLength(3)
            .WithMessage("Email or username must be at least 3 characters")
            .MaximumLength(256)
            .WithMessage("Email or username must not exceed 256 characters");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters");
    }
}
