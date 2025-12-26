using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.Login;

/// <summary>
/// Command to authenticate a user.
/// Tokens are set as httpOnly cookies - never returned in response body.
/// </summary>
public record LoginCommand : IRequest<Result>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email or username is required");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required");
    }
}
