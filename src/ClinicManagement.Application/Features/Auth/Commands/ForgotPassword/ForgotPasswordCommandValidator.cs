using ClinicManagement.Application.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(MessageCodes.Fields.EMAIL_REQUIRED)
            .EmailAddress().WithMessage(MessageCodes.Fields.EMAIL_INVALID_FORMAT);
    }
}
