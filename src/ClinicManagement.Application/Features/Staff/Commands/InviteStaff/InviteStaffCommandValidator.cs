using ClinicManagement.Domain.Common.Enums;
using FluentValidation;

namespace ClinicManagement.Application.Features.Staff.Commands.InviteStaff;

public class InviteStaffCommandValidator : AbstractValidator<InviteStaffCommand>
{
    public InviteStaffCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Invalid role")
            .Must(role => role == UserRole.Doctor || role == UserRole.Receptionist || role == UserRole.Nurse)
            .WithMessage("Only Doctor, Receptionist, or Nurse roles can be invited");

        RuleFor(x => x.SpecializationId)
            .NotNull().WithMessage("Specialization is required for doctors")
            .GreaterThan(0).WithMessage("Invalid specialization")
            .When(x => x.Role == UserRole.Doctor);

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number is not valid")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
