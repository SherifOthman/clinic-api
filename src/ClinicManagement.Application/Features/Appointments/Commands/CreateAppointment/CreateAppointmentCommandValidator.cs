using FluentValidation;

namespace ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.Appointment.ClinicBranchId)
            .NotEmpty()
            .WithMessage("Clinic branch is required");

        RuleFor(x => x.Appointment.PatientId)
            .NotEmpty()
            .WithMessage("Patient is required");

        RuleFor(x => x.Appointment.DoctorId)
            .NotEmpty()
            .WithMessage("Doctor is required");

        RuleFor(x => x.Appointment.AppointmentDate)
            .NotEmpty()
            .WithMessage("Appointment date is required")
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("Appointment date cannot be in the past");

        RuleFor(x => x.Appointment.AppointmentTypeId)
            .NotEmpty()
            .WithMessage("Appointment type is required");

        RuleFor(x => x.Appointment.CustomPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Appointment.CustomPrice.HasValue)
            .WithMessage("Custom price cannot be negative");

        RuleFor(x => x.Appointment.DiscountAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Discount amount cannot be negative");

        RuleFor(x => x.Appointment.PaidAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Paid amount cannot be negative");
    }
}