using FluentValidation;

namespace ClinicManagement.Application.Auth.Commands;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty()
            .WithMessage("Refresh token is required")
            .MinimumLength(32)
            .WithMessage("Invalid refresh token format");
    }
}
