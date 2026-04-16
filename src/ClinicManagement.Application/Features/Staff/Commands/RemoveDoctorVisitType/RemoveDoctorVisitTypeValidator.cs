using FluentValidation;

namespace ClinicManagement.Application.Features.Staff.Commands;

public class RemoveDoctorVisitTypeValidator : AbstractValidator<RemoveDoctorVisitTypeCommand>
{
    public RemoveDoctorVisitTypeValidator()
    {
        RuleFor(x => x.VisitTypeId)
            .NotEmpty().WithMessage("Visit type ID is required");
    }
}
