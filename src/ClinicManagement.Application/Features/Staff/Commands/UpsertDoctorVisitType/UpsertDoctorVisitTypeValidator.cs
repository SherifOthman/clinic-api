using FluentValidation;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class UpsertDoctorVisitTypeValidator : AbstractValidator<UpsertDoctorVisitTypeCommand>
{
    public UpsertDoctorVisitTypeValidator()
    {
        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required");

        RuleFor(x => x.BranchId)
            .NotEmpty().WithMessage("Branch ID is required");

        RuleFor(x => x.NameAr)
            .NotEmpty().WithMessage("Arabic name is required")
            .MaximumLength(100).WithMessage("Arabic name must not exceed 100 characters");

        RuleFor(x => x.NameEn)
            .NotEmpty().WithMessage("English name is required")
            .MaximumLength(100).WithMessage("English name must not exceed 100 characters");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be 0 or greater");
    }
}
