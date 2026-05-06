using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public class UpdateAppointmentHandler : IRequestHandler<UpdateAppointmentCommand, Result>
{
    private readonly IUnitOfWork _uow;

    public UpdateAppointmentHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result> Handle(UpdateAppointmentCommand request, CancellationToken ct)
    {
        var appointment = await _uow.Appointments.GetByIdForUpdateAsync(request.AppointmentId, ct);
        if (appointment is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Appointment not found");

        // Only allow editing active appointments
        if (appointment.Status is AppointmentStatus.Completed
            or AppointmentStatus.Cancelled
            or AppointmentStatus.NoShow)
            return Result.Failure(ErrorCodes.VALIDATION_ERROR, "Cannot edit a completed, cancelled, or no-show appointment");

        // Load visit type as a read-only projection to avoid EF tracking conflicts
        // (the appointment entity is already tracked; loading VisitType as a tracked entity
        //  would cause a duplicate-key error if the appointment's navigation is already cached)
        var visitTypeInfo = await _uow.DoctorSchedules.GetVisitTypePriceAsync(request.VisitTypeId, ct);
        if (visitTypeInfo is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Visit type not found or inactive");

        // Validate time slot for time-based appointments (exclude self)
        if (appointment.Type == AppointmentType.Time)
        {
            if (request.ScheduledTime is null)
                return Result.Failure(ErrorCodes.VALIDATION_ERROR, "Scheduled time is required for time-based appointments");

            var taken = await _uow.Appointments.TimeSlotTakenAsync(
                appointment.DoctorInfoId, appointment.Date, request.ScheduledTime.Value,
                appointment.Id, ct);
            if (taken)
                return Result.Failure(ErrorCodes.CONFLICT, "This time slot is already booked");
        }

        // Apply changes
        appointment.VisitTypeId          = request.VisitTypeId;
        appointment.VisitDurationMinutes = request.VisitDurationMinutes;

        if (appointment.Type == AppointmentType.Time && request.ScheduledTime.HasValue)
        {
            appointment.ScheduledTime = request.ScheduledTime;
            var duration = request.VisitDurationMinutes
                ?? visitTypeInfo.DefaultDoctorDurationMinutes
                ?? 30;
            appointment.EndTime = request.ScheduledTime.Value.AddMinutes(duration);
        }

        appointment.ApplyPrice(visitTypeInfo.Price, request.DiscountPercent);

        _uow.Appointments.Update(appointment);
        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
