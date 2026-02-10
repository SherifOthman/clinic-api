using ClinicManagement.Domain.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.ResendEmailVerification;

public class ResendEmailVerificationCommandValidator : AbstractValidator<ResendEmailVerificationCommand>
{
    public ResendEmailVerificationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithErrorCode(MessageCodes.Fields.EMAIL_REQUIRED)
            .EmailAddress()
            .WithErrorCode(MessageCodes.Fields.EMAIL_INVALID_FORMAT);
    }
}
