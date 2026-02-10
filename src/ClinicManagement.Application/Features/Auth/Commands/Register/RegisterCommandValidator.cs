using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    private readonly IPhoneValidationService _phoneValidationService;

    public RegisterCommandValidator(IPhoneValidationService phoneValidationService)
    {
        _phoneValidationService = phoneValidationService;

        RuleFor(x => x.FirstName)
            .NotEmpty().WithErrorCode(MessageCodes.Fields.FIRST_NAME_REQUIRED)
            .MinimumLength(2).WithErrorCode(MessageCodes.Fields.FIRST_NAME_MIN_LENGTH)
            .MaximumLength(50).WithErrorCode(MessageCodes.Fields.FIRST_NAME_MAX_LENGTH)
            .Matches(@"^[\u0600-\u06FFa-zA-Z\s'-]+$").WithErrorCode(MessageCodes.Fields.FIRST_NAME_INVALID_CHARACTERS);

        RuleFor(x => x.LastName)
            .NotEmpty().WithErrorCode(MessageCodes.Fields.LAST_NAME_REQUIRED)
            .MinimumLength(2).WithErrorCode(MessageCodes.Fields.LAST_NAME_MIN_LENGTH)
            .MaximumLength(50).WithErrorCode(MessageCodes.Fields.LAST_NAME_MAX_LENGTH)
            .Matches(@"^[\u0600-\u06FFa-zA-Z\s'-]+$").WithErrorCode(MessageCodes.Fields.LAST_NAME_INVALID_CHARACTERS);

        RuleFor(x => x.UserName)
            .NotEmpty().WithErrorCode(MessageCodes.Fields.USERNAME_REQUIRED)
            .MinimumLength(3).WithErrorCode(MessageCodes.Fields.USERNAME_MIN_LENGTH)
            .MaximumLength(30).WithErrorCode(MessageCodes.Fields.USERNAME_MAX_LENGTH)
            .Matches("^[a-zA-Z0-9_]+$").WithErrorCode(MessageCodes.Fields.USERNAME_INVALID_CHARACTERS)
            .Must(username => string.IsNullOrEmpty(username) || (!username.StartsWith("_") && !username.EndsWith("_")))
            .WithErrorCode(MessageCodes.Fields.USERNAME_UNDERSCORE_POSITION);

        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode(MessageCodes.Fields.EMAIL_REQUIRED)
            .EmailAddress().WithErrorCode(MessageCodes.Fields.EMAIL_INVALID_FORMAT)
            .MaximumLength(254).WithErrorCode(MessageCodes.Fields.EMAIL_MAX_LENGTH);

        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode(MessageCodes.Fields.PASSWORD_REQUIRED)
            .MinimumLength(8).WithErrorCode(MessageCodes.Fields.PASSWORD_MIN_LENGTH)
            .MaximumLength(128).WithErrorCode(MessageCodes.Fields.PASSWORD_MAX_LENGTH)
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithErrorCode(MessageCodes.Fields.PASSWORD_COMPLEXITY)
            .Must(password => string.IsNullOrEmpty(password) || !password.Contains(" ")).WithErrorCode(MessageCodes.Fields.PASSWORD_NO_SPACES);

        RuleFor(x => x.PhoneNumber)
            .Must(BeValidPhoneNumber).WithErrorCode(MessageCodes.Fields.PHONE_NUMBER_INVALID)
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }

    private bool BeValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber)) return true;
        
        var result = _phoneValidationService.ValidatePhoneNumber(phoneNumber);
        return result.IsValid;
    }
}
