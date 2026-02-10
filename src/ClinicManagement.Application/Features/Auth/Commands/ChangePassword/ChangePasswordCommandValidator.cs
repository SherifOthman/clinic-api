using ClinicManagement.Domain.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithErrorCode(MessageCodes.Fields.CURRENT_PASSWORD_REQUIRED);

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithErrorCode(MessageCodes.Fields.NEW_PASSWORD_REQUIRED)
            .MinimumLength(8).WithErrorCode(MessageCodes.Fields.PASSWORD_MIN_LENGTH)
            .MaximumLength(128).WithErrorCode(MessageCodes.Fields.PASSWORD_MAX_LENGTH)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithErrorCode(MessageCodes.Fields.PASSWORD_COMPLEXITY)
            .Must(password => !password.Contains(" ")).WithErrorCode(MessageCodes.Fields.PASSWORD_NO_SPACES)
            .NotEqual(x => x.CurrentPassword).WithErrorCode(MessageCodes.Fields.PASSWORD_DIFFERENT_REQUIRED);
    }
}
