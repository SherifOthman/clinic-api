using ClinicManagement.Application.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage(MessageCodes.Fields.CURRENT_PASSWORD_REQUIRED);

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(MessageCodes.Fields.NEW_PASSWORD_REQUIRED)
            .MinimumLength(8).WithMessage(MessageCodes.Fields.PASSWORD_MIN_LENGTH)
            .MaximumLength(128).WithMessage(MessageCodes.Fields.PASSWORD_MAX_LENGTH)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage(MessageCodes.Fields.PASSWORD_COMPLEXITY)
            .Must(password => !password.Contains(" ")).WithMessage(MessageCodes.Fields.PASSWORD_NO_SPACES)
            .NotEqual(x => x.CurrentPassword).WithMessage(MessageCodes.Fields.PASSWORD_DIFFERENT_REQUIRED);

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage(MessageCodes.Fields.CONFIRM_PASSWORD_REQUIRED)
            .Equal(x => x.NewPassword).WithMessage(MessageCodes.Fields.PASSWORDS_MUST_MATCH);
    }
}
