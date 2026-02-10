using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.MedicalSupplies.Commands.CreateMedicalSupply;

public class CreateMedicalSupplyCommandValidator : AbstractValidator<CreateMedicalSupplyCommand>
{
    public CreateMedicalSupplyCommandValidator()
    {
        RuleFor(x => x.ClinicBranchId)
            .NotEmpty().WithErrorCode(MessageCodes.Common.CLINIC_BRANCH_REQUIRED);

        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode(MessageCodes.MedicalSupply.NAME_REQUIRED)
            .MaximumLength(200).WithErrorCode(MessageCodes.MedicalSupply.NAME_TOO_LONG);

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithErrorCode(MessageCodes.MedicalSupply.PRICE_MUST_BE_POSITIVE);

        RuleFor(x => x.QuantityInStock)
            .GreaterThanOrEqualTo(0).WithErrorCode(MessageCodes.MedicalSupply.STOCK_CANNOT_BE_NEGATIVE);

        RuleFor(x => x.MinimumStockLevel)
            .GreaterThanOrEqualTo(0).WithErrorCode(MessageCodes.MedicalSupply.MINIMUM_STOCK_CANNOT_BE_NEGATIVE);
    }
}