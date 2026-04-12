using ClinicManagement.Application.Common.Validators;
using FluentValidation;

namespace ClinicManagement.Application.Features.Patients.Commands;

public class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    private static readonly HashSet<string> ValidBloodTypes =
        ["A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"];

    public CreatePatientCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(200)
            .Matches(@"^[\u0600-\u06FFa-zA-Z\s'\-\.]+$")
            .WithMessage("Full name must contain only letters, spaces, hyphens, apostrophes, or dots");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .Must(BeAValidDate).WithMessage("Invalid date of birth format (use YYYY-MM-DD)")
            .Must(BeInThePast).WithMessage("Date of birth must be in the past");

        RuleFor(x => x.Gender)
            .NotEmpty()
            .Must(g => g == "Male" || g == "Female")
            .WithMessage("Gender must be Male or Female");

        RuleFor(x => x.BloodType)
            .Must(bt => bt == null || ValidBloodTypes.Contains(bt))
            .WithMessage("Invalid blood type");

        RuleFor(x => x.PhoneNumbers)
            .Must(phones => phones == null || phones.Count == 0 || phones.All(p => !string.IsNullOrWhiteSpace(p)))
            .WithMessage("Phone numbers cannot be empty strings");

        RuleForEach(x => x.PhoneNumbers)
            .NotEmpty()
            .MustBeValidPhoneNumber()
            .WithMessage("Invalid phone number format");
    }

    private static bool BeAValidDate(string? value) =>
        !string.IsNullOrEmpty(value) && DateOnly.TryParse(value, out _);

    private static bool BeInThePast(string? value) =>
        DateOnly.TryParse(value, out var date) && date < DateOnly.FromDateTime(DateTime.UtcNow);
}
