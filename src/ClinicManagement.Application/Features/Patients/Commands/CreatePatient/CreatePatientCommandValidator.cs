using FluentValidation;

namespace ClinicManagement.Application.Features.Patients.Commands.CreatePatient;

public class CreatePatientCommandValidator : AbstractValidator<CreatePatientCommand>
{
    public CreatePatientCommandValidator()
    {
        RuleFor(x => x.Dto.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Dto.Gender)
            .IsInEnum();

        RuleFor(x => x.Dto.DateOfBirth)
            .NotEmpty()
            .LessThan(DateTime.UtcNow);

        RuleFor(x => x.Dto.PhoneNumbers)
            .NotEmpty()
            .WithMessage("At least one phone number is required");

        RuleForEach(x => x.Dto.PhoneNumbers)
            .ChildRules(phone =>
            {
                phone.RuleFor(p => p.PhoneNumber)
                    .NotEmpty()
                    .MaximumLength(20);
            });
    }
}
