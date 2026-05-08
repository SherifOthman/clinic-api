using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using FluentValidation;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class InviteStaffValidator : AbstractValidator<InviteStaffCommand>
{
    public InviteStaffValidator(IReferenceRepository reference)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required")
            .Must(r => r == UserRoles.Doctor || r == UserRoles.Receptionist)
            .WithMessage($"Role must be either {UserRoles.Doctor} or {UserRoles.Receptionist}");

        RuleFor(x => x.SpecializationId)
            .NotNull().When(x => x.Role == UserRoles.Doctor)
            .WithMessage("Specialization is required for doctors");

        When(x => x.Role == UserRoles.Doctor && x.SpecializationId.HasValue, () =>
        {
            RuleFor(x => x.SpecializationId!.Value)
                .MustAsync(async (id, ct) => await reference.SpecializationExistsAsync(id, ct))
                .WithErrorCode("NOT_FOUND")
                .WithMessage("Specialization not found");
        });
    }
}
