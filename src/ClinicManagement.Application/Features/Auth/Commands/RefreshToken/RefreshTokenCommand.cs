using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Auth.Commands.Login;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand : IRequest<Result<AuthResponseDto>>
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class LoginCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();

    }
}
