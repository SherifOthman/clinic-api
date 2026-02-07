using ClinicManagement.Domain.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode(MessageCodes.Fields.EMAIL_REQUIRED)
            .EmailAddress().WithErrorCode(MessageCodes.Fields.EMAIL_INVALID_FORMAT);

        RuleFor(x => x.Token)
            .NotEmpty().WithErrorCode(MessageCodes.Authentication.INVALID_RESET_TOKEN);

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithErrorCode(MessageCodes.Fields.PASSWORD_REQUIRED)
            .MinimumLength(8).WithErrorCode(MessageCodes.Fields.PASSWORD_MIN_LENGTH)
            .MaximumLength(128).WithErrorCode(MessageCodes.Fields.PASSWORD_MAX_LENGTH)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithErrorCode(MessageCodes.Fields.PASSWORD_COMPLEXITY)
            .Must(password => !password.Contains(" ")).WithErrorCode(MessageCodes.Fields.PASSWORD_NO_SPACES);
    }
}
