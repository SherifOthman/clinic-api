using FluentValidation;

namespace ClinicManagement.Application.Features.Staff.Commands.AcceptInvitation;

public class AcceptInvitationCommandValidator : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("Invalid user ID");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Invitation token is required");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MinimumLength(2).WithMessage("First name must be at least 2 characters")
            .MaximumLength(50).WithMessage("First name must be less than 50 characters")
            .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("First name can only contain letters, spaces, hyphens, and apostrophes");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MinimumLength(2).WithMessage("Last name must be at least 2 characters")
            .MaximumLength(50).WithMessage("Last name must be less than 50 characters")
            .Matches(@"^[a-zA-Z\s'-]+$").WithMessage("Last name can only contain letters, spaces, hyphens, and apostrophes");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .MaximumLength(128).WithMessage("Password must be less than 128 characters")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)").WithMessage("Password must contain at least one uppercase letter, one lowercase letter, and one number")
            .Must(password => !password.Contains(" ")).WithMessage("Password cannot contain spaces");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[\d\s\-\(\)]+$").WithMessage("Please enter a valid phone number")
            .Must(phone => 
            {
                if (string.IsNullOrEmpty(phone)) return true; // Optional field
                var digitsOnly = System.Text.RegularExpressions.Regex.Replace(phone, @"\D", "");
                return digitsOnly.Length >= 10 && digitsOnly.Length <= 15;
            }).WithMessage("Phone number must contain 10-15 digits")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
