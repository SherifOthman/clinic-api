using ClinicManagement.Application.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(MessageCodes.Fields.EMAIL_REQUIRED)
            .EmailAddress().WithMessage(MessageCodes.Fields.EMAIL_INVALID_FORMAT);

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(MessageCodes.Authentication.INVALID_RESET_TOKEN);

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(MessageCodes.Fields.PASSWORD_REQUIRED)
            .MinimumLength(8).WithMessage(MessageCodes.Fields.PASSWORD_MIN_LENGTH)
            .MaximumLength(128).WithMessage(MessageCodes.Fields.PASSWORD_MAX_LENGTH)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage(MessageCodes.Fields.PASSWORD_COMPLEXITY)
            .Must(password => !password.Contains(" ")).WithMessage(MessageCodes.Fields.PASSWORD_NO_SPACES);

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage(MessageCodes.Fields.CONFIRM_PASSWORD_REQUIRED)
            .Equal(x => x.NewPassword).WithMessage(MessageCodes.Fields.PASSWORDS_MUST_MATCH);
    }
}
