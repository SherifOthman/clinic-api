using ClinicManagement.Application.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage(MessageCodes.Fields.FIRST_NAME_REQUIRED)
            .MinimumLength(2).WithMessage(MessageCodes.Fields.FIRST_NAME_MIN_LENGTH)
            .MaximumLength(50).WithMessage(MessageCodes.Fields.FIRST_NAME_MAX_LENGTH)
            .Matches(@"^[\u0600-\u06FFa-zA-Z\s'-]+$").WithMessage(MessageCodes.Fields.FIRST_NAME_INVALID_CHARACTERS);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage(MessageCodes.Fields.LAST_NAME_REQUIRED)
            .MinimumLength(2).WithMessage(MessageCodes.Fields.LAST_NAME_MIN_LENGTH)
            .MaximumLength(50).WithMessage(MessageCodes.Fields.LAST_NAME_MAX_LENGTH)
            .Matches(@"^[\u0600-\u06FFa-zA-Z\s'-]+$").WithMessage(MessageCodes.Fields.LAST_NAME_INVALID_CHARACTERS);

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage(MessageCodes.Fields.USERNAME_REQUIRED)
            .MinimumLength(3).WithMessage(MessageCodes.Fields.USERNAME_MIN_LENGTH)
            .MaximumLength(30).WithMessage(MessageCodes.Fields.USERNAME_MAX_LENGTH)
            .Matches("^[a-zA-Z0-9_]+$").WithMessage(MessageCodes.Fields.USERNAME_INVALID_CHARACTERS)
            .Must(username => string.IsNullOrEmpty(username) || (!username.StartsWith("_") && !username.EndsWith("_")))
            .WithMessage(MessageCodes.Fields.USERNAME_UNDERSCORE_POSITION);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(MessageCodes.Fields.EMAIL_REQUIRED)
            .EmailAddress().WithMessage(MessageCodes.Fields.EMAIL_INVALID_FORMAT)
            .MaximumLength(254).WithMessage(MessageCodes.Fields.EMAIL_MAX_LENGTH);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(MessageCodes.Fields.PASSWORD_REQUIRED)
            .MinimumLength(8).WithMessage(MessageCodes.Fields.PASSWORD_MIN_LENGTH)
            .MaximumLength(128).WithMessage(MessageCodes.Fields.PASSWORD_MAX_LENGTH)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage(MessageCodes.Fields.PASSWORD_COMPLEXITY)
            .Must(password => string.IsNullOrEmpty(password) || !password.Contains(" ")).WithMessage(MessageCodes.Fields.PASSWORD_NO_SPACES);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage(MessageCodes.Fields.PHONE_NUMBER_MAX_LENGTH)
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
