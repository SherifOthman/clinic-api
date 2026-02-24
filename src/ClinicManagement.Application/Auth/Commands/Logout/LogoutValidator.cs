using FluentValidation;

namespace ClinicManagement.Application.Auth.Commands;

public class LogoutValidator : AbstractValidator<LogoutCommand>
{
    public LogoutValidator()
    {
        RuleFor(x => x.RefreshToken)
            .MinimumLength(32)
            .When(x => !string.IsNullOrEmpty(x.RefreshToken))
            .WithMessage("Invalid refresh token format");
    }
}
