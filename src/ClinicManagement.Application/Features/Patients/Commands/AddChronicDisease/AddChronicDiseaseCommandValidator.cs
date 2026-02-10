using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Patients.Commands.AddChronicDisease;

public class AddChronicDiseaseCommandValidator : AbstractValidator<AddChronicDiseaseCommand>
{
    public AddChronicDiseaseCommandValidator()
    {
        RuleFor(x => x.PatientId)
            .NotEmpty()
            .WithErrorCode(MessageCodes.Patient.ID_REQUIRED);

        RuleFor(x => x.ChronicDisease.ChronicDiseaseId)
            .NotEmpty()
            .WithErrorCode(MessageCodes.ChronicDisease.NOT_FOUND);

        RuleFor(x => x.ChronicDisease.Status)
            .MaximumLength(50)
            .WithErrorCode(MessageCodes.ChronicDisease.STATUS_TOO_LONG);

        RuleFor(x => x.ChronicDisease.Notes)
            .MaximumLength(1000)
            .WithErrorCode(MessageCodes.ChronicDisease.NOTES_TOO_LONG);

        RuleFor(x => x.ChronicDisease.DiagnosedDate)
            .LessThanOrEqualTo(DateTime.Today)
            .When(x => x.ChronicDisease.DiagnosedDate.HasValue)
            .WithErrorCode(MessageCodes.ChronicDisease.DIAGNOSED_DATE_IN_FUTURE);
    }
}