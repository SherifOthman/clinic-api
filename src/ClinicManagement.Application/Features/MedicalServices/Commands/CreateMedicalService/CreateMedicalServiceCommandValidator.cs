using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.MedicalServices.Commands.CreateMedicalService;

public class CreateMedicalServiceCommandValidator : AbstractValidator<CreateMedicalServiceCommand>
{
    public CreateMedicalServiceCommandValidator()
    {
        RuleFor(x => x.ClinicBranchId)
            .NotEmpty().WithErrorCode(MessageCodes.Common.CLINIC_BRANCH_REQUIRED);

        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode(MessageCodes.MedicalService.NAME_REQUIRED)
            .MaximumLength(200).WithErrorCode(MessageCodes.MedicalService.NAME_TOO_LONG);

        RuleFor(x => x.DefaultPrice)
            .GreaterThan(0).WithErrorCode(MessageCodes.MedicalService.PRICE_MUST_BE_POSITIVE);
    }
}