using FluentValidation;

namespace ClinicManagement.Application.Features.MedicalServices.Commands.CreateMedicalService;

public class CreateMedicalServiceCommandValidator : AbstractValidator<CreateMedicalServiceCommand>
{
    public CreateMedicalServiceCommandValidator()
    {
        RuleFor(x => x.ClinicBranchId)
            .NotEmpty().WithMessage("Clinic branch is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Medical service name is required")
            .MaximumLength(200).WithMessage("Medical service name cannot exceed 200 characters");

        RuleFor(x => x.DefaultPrice)
            .GreaterThan(0).WithMessage("Default price must be greater than zero");
    }
}