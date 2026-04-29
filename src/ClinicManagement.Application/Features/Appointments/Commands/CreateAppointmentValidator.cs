using ClinicManagement.Domain.Enums;
using FluentValidation;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public class CreateAppointmentValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.PatientId).NotEmpty();
        RuleFor(x => x.DoctorInfoId).NotEmpty();
        RuleFor(x => x.VisitTypeId).NotEmpty();
        RuleFor(x => x.Date).NotEmpty();

        RuleFor(x => x.ScheduledTime)
            .NotNull()
            .WithMessage("Scheduled time is required for time-based appointments")
            .When(x => x.Type == AppointmentType.Time);

        RuleFor(x => x.ScheduledTime)
            .Null()
            .WithMessage("Scheduled time must be empty for queue-based appointments")
            .When(x => x.Type == AppointmentType.Queue);

        RuleFor(x => x.DiscountPercent)
            .InclusiveBetween(0, 100)
            .When(x => x.DiscountPercent.HasValue);
    }
}
