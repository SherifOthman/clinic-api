using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Medicines.Commands.CreateMedicine;

public class CreateMedicineCommandValidator : AbstractValidator<CreateMedicineCommand>
{
    public CreateMedicineCommandValidator()
    {
        RuleFor(x => x.ClinicBranchId)
            .NotEmpty().WithErrorCode(MessageCodes.Common.CLINIC_BRANCH_REQUIRED);

        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode(MessageCodes.Medicine.NAME_REQUIRED)
            .MaximumLength(200).WithErrorCode(MessageCodes.Medicine.NAME_TOO_LONG);

        RuleFor(x => x.BoxPrice)
            .GreaterThan(0).WithErrorCode(MessageCodes.Medicine.PRICE_MUST_BE_POSITIVE);

        RuleFor(x => x.StripsPerBox)
            .GreaterThan(0).WithErrorCode(MessageCodes.Medicine.STRIPS_PER_BOX_MUST_BE_POSITIVE);

        RuleFor(x => x.TotalStripsInStock)
            .GreaterThanOrEqualTo(0).WithErrorCode(MessageCodes.Medicine.STOCK_CANNOT_BE_NEGATIVE);

        RuleFor(x => x.MinimumStockLevel)
            .GreaterThanOrEqualTo(0).WithErrorCode(MessageCodes.Medicine.MINIMUM_STOCK_CANNOT_BE_NEGATIVE);
    }
}