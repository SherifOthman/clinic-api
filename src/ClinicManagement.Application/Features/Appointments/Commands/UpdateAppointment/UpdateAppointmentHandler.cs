using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public class UpdateAppointmentHandler : IRequestHandler<UpdateAppointmentCommand, Result>
{
    private readonly IAppointmentRepository    _appointments;
    private readonly IDoctorScheduleRepository _schedules;
    private readonly IUnitOfWork _uow;

    public UpdateAppointmentHandler(
        IAppointmentRepository appointments,
        IDoctorScheduleRepository schedules,
        IUnitOfWork uow)
    {
        _appointments = appointments;
        _schedules    = schedules;
        _uow          = uow;
    }

    public async Task<Result> Handle(UpdateAppointmentCommand request, CancellationToken ct)
    {
        var appointment = await _appointments.GetByIdForUpdateAsync(request.AppointmentId, ct);
        if (appointment is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Appointment not found");

        if (appointment.Status is AppointmentStatus.Completed
            or AppointmentStatus.Cancelled
            or AppointmentStatus.NoShow)
            return Result.Failure(ErrorCodes.VALIDATION_ERROR, "Cannot edit a completed, cancelled, or no-show appointment");

        var visitTypeInfo = await _schedules.GetVisitTypePriceAsync(request.VisitTypeId, ct);
        if (visitTypeInfo is null)
            return Result.Failure(ErrorCodes.NOT_FOUND, "Visit type not found or inactive");

        if (appointment.Type == AppointmentType.Time)
        {
            if (request.ScheduledTime is null)
                return Result.Failure(ErrorCodes.VALIDATION_ERROR, "Scheduled time is required for time-based appointments");

            var taken = await _appointments.TimeSlotTakenAsync(
                appointment.DoctorInfoId, appointment.Date, request.ScheduledTime.Value,
                appointment.Id, ct);
            if (taken)
                return Result.Failure(ErrorCodes.CONFLICT, "This time slot is already booked");
        }

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

        _appointments.Update(appointment);
        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
