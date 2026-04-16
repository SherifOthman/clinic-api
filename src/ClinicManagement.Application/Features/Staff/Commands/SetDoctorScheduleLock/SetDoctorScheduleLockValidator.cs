using FluentValidation;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class SetDoctorScheduleLockValidator : AbstractValidator<SetDoctorScheduleLockCommand>
{
    public SetDoctorScheduleLockValidator()
    {
        RuleFor(x => x.StaffId)
            .NotEmpty().WithMessage("Staff ID is required");
    }
}
