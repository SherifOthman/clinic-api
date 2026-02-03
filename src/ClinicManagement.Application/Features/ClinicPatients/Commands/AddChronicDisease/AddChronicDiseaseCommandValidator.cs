using FluentValidation;

namespace ClinicManagement.Application.Features.ClinicPatients.Commands.AddChronicDisease;

public class AddChronicDiseaseCommandValidator : AbstractValidator<AddChronicDiseaseCommand>
{
    public AddChronicDiseaseCommandValidator()
    {
        RuleFor(x => x.ClinicPatientId)
            .NotEmpty()
            .WithMessage("Clinic patient ID is required");

        RuleFor(x => x.ChronicDisease.ChronicDiseaseId)
            .NotEmpty()
            .WithMessage("Chronic disease ID is required");

        RuleFor(x => x.ChronicDisease.Status)
            .MaximumLength(50)
            .WithMessage("Status cannot exceed 50 characters");

        RuleFor(x => x.ChronicDisease.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes cannot exceed 1000 characters");

        RuleFor(x => x.ChronicDisease.DiagnosedDate)
            .LessThanOrEqualTo(DateTime.Today)
            .When(x => x.ChronicDisease.DiagnosedDate.HasValue)
            .WithMessage("Diagnosed date cannot be in the future");
    }
}