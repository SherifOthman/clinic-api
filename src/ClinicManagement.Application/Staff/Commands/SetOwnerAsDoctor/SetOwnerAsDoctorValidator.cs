using FluentValidation;

namespace ClinicManagement.Application.Staff.Commands;

public class SetOwnerAsDoctorValidator : AbstractValidator<SetOwnerAsDoctor>
{
    public SetOwnerAsDoctorValidator()
    {
        RuleFor(x => x.YearsOfExperience)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Years of experience must be a positive number");

        RuleFor(x => x.LicenseNumber)
            .MaximumLength(50)
            .When(x => !string.IsNullOrEmpty(x.LicenseNumber))
            .WithMessage("License number must not exceed 50 characters");
    }
}
