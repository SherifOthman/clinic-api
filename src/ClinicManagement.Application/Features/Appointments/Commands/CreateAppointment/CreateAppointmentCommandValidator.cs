using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Constants;
using FluentValidation;

namespace ClinicManagement.Application.Features.Appointments.Commands.CreateAppointment;

public class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.Appointment.ClinicBranchId)
            .NotEmpty()
            .WithErrorCode(MessageCodes.Common.CLINIC_BRANCH_REQUIRED);

        RuleFor(x => x.Appointment.PatientId)
            .NotEmpty()
            .WithErrorCode(MessageCodes.Appointment.PATIENT_REQUIRED);

        RuleFor(x => x.Appointment.DoctorId)
            .NotEmpty()
            .WithErrorCode(MessageCodes.Appointment.DOCTOR_REQUIRED);

        RuleFor(x => x.Appointment.AppointmentDate)
            .NotEmpty()
            .WithErrorCode(MessageCodes.Appointment.DATE_REQUIRED)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithErrorCode(MessageCodes.Appointment.DATE_IN_PAST);

        RuleFor(x => x.Appointment.AppointmentTypeId)
            .NotEmpty()
            .WithErrorCode(MessageCodes.Appointment.TYPE_REQUIRED);

        RuleFor(x => x.Appointment.CustomPrice)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Appointment.CustomPrice.HasValue)
            .WithErrorCode(MessageCodes.Appointment.PRICE_CANNOT_BE_NEGATIVE);

        RuleFor(x => x.Appointment.DiscountAmount)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode(MessageCodes.Appointment.DISCOUNT_CANNOT_BE_NEGATIVE);

        RuleFor(x => x.Appointment.PaidAmount)
            .GreaterThanOrEqualTo(0)
            .WithErrorCode(MessageCodes.Appointment.PAID_AMOUNT_CANNOT_BE_NEGATIVE);
    }
}