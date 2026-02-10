using FluentValidation;

namespace ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;

public class UpdatePatientCommandValidator : AbstractValidator<UpdatePatientCommand>
{
    public UpdatePatientCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Patient ID is required");

        RuleFor(x => x.Dto.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(200).WithMessage("Full name cannot exceed 200 characters");

        RuleFor(x => x.Dto.Gender)
            .IsInEnum().WithMessage("Invalid gender");

        RuleFor(x => x.Dto.DateOfBirth)
            .NotEmpty().WithMessage("Date of birth is required")
            .LessThan(DateTime.UtcNow).WithMessage("Date of birth cannot be in the future");

        RuleFor(x => x.Dto.PhoneNumbers)
            .NotEmpty()
            .WithMessage("At least one phone number is required");

        RuleForEach(x => x.Dto.PhoneNumbers)
            .ChildRules(phone =>
            {
                phone.RuleFor(p => p.PhoneNumber)
                    .NotEmpty().WithMessage("Phone number is required")
                    .MaximumLength(20).WithMessage("Phone number cannot exceed 20 characters");
            });
    }
}
