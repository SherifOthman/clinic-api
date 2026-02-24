using FluentValidation;

namespace ClinicManagement.Application.Staff.Commands;

public class InviteStaffValidator : AbstractValidator<InviteStaffCommand>
{
    public InviteStaffValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(256)
            .WithMessage("Email must not exceed 256 characters");

        RuleFor(x => x.Role)
            .NotEmpty()
            .WithMessage("Role is required")
            .Must(role => role == "Doctor" || role == "Receptionist")
            .WithMessage("Role must be either Doctor or Receptionist");

        RuleFor(x => x.SpecializationId)
            .NotEmpty()
            .When(x => x.SpecializationId.HasValue)
            .WithMessage("Specialization ID must be provided");

        RuleFor(x => x.SpecializationId)
            .NotNull()
            .When(x => x.Role == "Doctor")
            .WithMessage("Specialization is required for doctors");
    }
}
