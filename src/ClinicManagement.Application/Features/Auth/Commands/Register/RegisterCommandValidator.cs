using ClinicManagement.Application.Common.Interfaces;
using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    private readonly IPhoneValidationService _phoneValidationService;

    public RegisterCommandValidator(IPhoneValidationService phoneValidationService)
    {
        _phoneValidationService = phoneValidationService;

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MinimumLength(2).WithMessage("First name must be at least 2 characters")
            .MaximumLength(50).WithMessage("First name cannot exceed 50 characters")
            .Matches(@"^[\u0600-\u06FFa-zA-Z\s'-]+$").WithMessage("First name contains invalid characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters")
            .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters")
            .Matches(@"^[\u0600-\u06FFa-zA-Z\s'-]+$").WithMessage("Last name contains invalid characters");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(30).WithMessage("Username cannot exceed 30 characters")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores")
            .Must(username => string.IsNullOrEmpty(username) || (!username.StartsWith("_") && !username.EndsWith("_")))
            .WithMessage("Username cannot start or end with an underscore");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is invalid")
            .MaximumLength(254).WithMessage("Email cannot exceed 254 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(128).WithMessage("Password cannot exceed 128 characters")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("Password must contain at least one uppercase letter, one lowercase letter, and one number")
            .Must(password => string.IsNullOrEmpty(password) || !password.Contains(" ")).WithMessage("Password cannot contain spaces");

        RuleFor(x => x.PhoneNumber)
            .Must(BeValidPhoneNumber).WithMessage("Phone number format is invalid")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }

    private bool BeValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber)) return true;
        
        var result = _phoneValidationService.ValidatePhoneNumber(phoneNumber);
        return result.IsValid;
    }
}
