using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.Login;

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
            .NotEmpty().WithMessage(MessageCodes.Fields.EMAIL_REQUIRED)
            .EmailAddress().WithMessage(MessageCodes.Fields.EMAIL_INVALID_FORMAT);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(MessageCodes.Fields.PASSWORD_REQUIRED)
            .MinimumLength(6).WithMessage(MessageCodes.Fields.PASSWORD_MIN_LENGTH);
    }
}
