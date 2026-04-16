using ClinicManagement.Application.Abstractions.Data;
using FluentValidation;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class InviteStaffValidator : AbstractValidator<InviteStaffCommand>
{
    public InviteStaffValidator(IUnitOfWork uow)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(r => r == "Doctor" || r == "Receptionist")
            .WithMessage("Role must be either Doctor or Receptionist");

        RuleFor(x => x.SpecializationId)
            .NotNull().When(x => x.Role == "Doctor")
            .WithMessage("Specialization is required for doctors");

        When(x => x.Role == "Doctor" && x.SpecializationId.HasValue, () =>
        {
            RuleFor(x => x.SpecializationId!.Value)
                .MustAsync(async (id, ct) => await uow.Reference.SpecializationExistsAsync(id, ct))
                .WithErrorCode("NOT_FOUND")
                .WithMessage("Specialization not found");
        });
    }
}
