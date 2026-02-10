using FluentValidation;

namespace ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;

public class UpdatePatientCommandValidator : AbstractValidator<UpdatePatientCommand>
{
    public UpdatePatientCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

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
