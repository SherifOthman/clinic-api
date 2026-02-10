using ClinicManagement.Domain.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode(MessageCodes.Fields.EMAIL_REQUIRED)
            .EmailAddress().WithErrorCode(MessageCodes.Fields.EMAIL_INVALID_FORMAT);
    }
}
