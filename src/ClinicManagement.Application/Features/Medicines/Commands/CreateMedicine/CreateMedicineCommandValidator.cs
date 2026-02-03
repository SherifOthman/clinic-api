using FluentValidation;

namespace ClinicManagement.Application.Features.Medicines.Commands.CreateMedicine;

public class CreateMedicineCommandValidator : AbstractValidator<CreateMedicineCommand>
{
    public CreateMedicineCommandValidator()
    {
        RuleFor(x => x.ClinicBranchId)
            .NotEmpty().WithMessage("Clinic branch is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Medicine name is required.")
            .MaximumLength(200).WithMessage("Medicine name must not exceed 200 characters.");

        RuleFor(x => x.BoxPrice)
            .GreaterThan(0).WithMessage("Box price must be greater than 0.");

        RuleFor(x => x.StripsPerBox)
            .GreaterThan(0).WithMessage("Strips per box must be greater than 0.");

        RuleFor(x => x.TotalStripsInStock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative.");

        RuleFor(x => x.MinimumStockLevel)
            .GreaterThanOrEqualTo(0).WithMessage("Minimum stock level cannot be negative.");
    }
}