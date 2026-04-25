using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Validators;
using FluentValidation;

namespace ClinicManagement.Application.Features.Auth.Commands.Register;

public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator(IUnitOfWork uow)
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.UserName)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(50)
            .MustAsync(async (username, ct) => !await uow.Users.AnyByUsernameAsync(username, ct))
            .WithErrorCode("USERNAME_ALREADY_EXISTS")
            .WithMessage("Username is already taken");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible)
            .MaximumLength(100)
            .MustAsync(async (email, ct) => !await uow.Users.AnyByEmailAsync(email, ct))
            .WithErrorCode("EMAIL_ALREADY_EXISTS")
            .WithMessage("Email is already registered");

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .Matches("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).+$")
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, and one number.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .MustBeValidPhoneNumber();

        RuleFor(x => x.Gender)
            .NotEmpty()
            .Must(g => g == "Male" || g == "Female")
            .WithMessage("Gender must be either Male or Female");
    }
}
