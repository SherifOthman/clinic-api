using ClinicManagement.Domain.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.Appointment.ClinicBranchId)
            .NotEmpty()
            .WithMessage(MessageCodes.Common.CLINIC_BRANCH_REQUIRED);

        RuleFor(x => x.Appointment.PatientId)
            .NotEmpty()
            .WithMessage(MessageCodes.Appointment.PATIENT_REQUIRED);

        RuleFor(x => x.Appointment.DoctorId)
            .NotEmpty()
            .WithMessage(MessageCodes.Appointment.DOCTOR_REQUIRED);

        RuleFor(x => x.Appointment.AppointmentDate)
            .NotEmpty()
            .WithMessage(MessageCodes.Appointment.DATE_REQUIRED)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage(MessageCodes.Appointment.DATE_IN_PAST);

        RuleFor(x => x.Appointment.AppointmentTypeId)
            .NotEmpty()
            .WithMessage(MessageCodes.Appointment.TYPE_REQUIRED);

        RuleFor(x => x.Appointment.CustomPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Appointment.CustomPrice.HasValue)
            .WithMessage(MessageCodes.Appointment.PRICE_CANNOT_BE_NEGATIVE);

        RuleFor(x => x.Appointment.DiscountAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(MessageCodes.Appointment.DISCOUNT_CANNOT_BE_NEGATIVE);

        RuleFor(x => x.Appointment.PaidAmount)
            .GreaterThanOrEqualTo(0)
            .WithMessage(MessageCodes.Appointment.PAID_AMOUNT_CANNOT_BE_NEGATIVE);
    }
}
