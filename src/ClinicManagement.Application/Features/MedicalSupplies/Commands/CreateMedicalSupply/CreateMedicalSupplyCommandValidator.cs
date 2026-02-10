using FluentValidation;

namespace ClinicManagement.Application.Features.MedicalSupplies.Commands.CreateMedicalSupply;

public class CreateMedicalSupplyCommandValidator : AbstractValidator<CreateMedicalSupplyCommand>
{
    public CreateMedicalSupplyCommandValidator()
    {
        RuleFor(x => x.ClinicBranchId)
            .NotEmpty().WithMessage("Clinic branch is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Medical supply name is required")
            .MaximumLength(200).WithMessage("Medical supply name cannot exceed 200 characters");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than zero");

        RuleFor(x => x.QuantityInStock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative");

        RuleFor(x => x.MinimumStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock level cannot be negative");
    }
}