using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public class CreateAppointmentHandler : IRequestHandler<CreateAppointmentCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public CreateAppointmentHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateAppointmentCommand request, CancellationToken ct)
    {
        var clinicId = _currentUser.GetRequiredClinicId();

        // Validate visit type belongs to this doctor
        var visitType = await _uow.DoctorSchedules.GetVisitTypeByIdAsync(request.VisitTypeId, ct);
        if (visitType is null || !visitType.IsActive)
            return Result.Failure<Guid>(ErrorCodes.NOT_FOUND, "Visit type not found or inactive");

        // Validate time slot for time-based appointments
        if (request.Type == AppointmentType.Time)
        {
            if (request.ScheduledTime is null)
                return Result.Failure<Guid>(ErrorCodes.VALIDATION_ERROR, "Scheduled time is required");

            var taken = await _uow.Appointments.TimeSlotTakenAsync(
                request.DoctorInfoId, request.Date, request.ScheduledTime.Value, null, ct);
            if (taken)
                return Result.Failure<Guid>(ErrorCodes.CONFLICT, "This time slot is already booked");
        }

        var appointment = new Appointment
        {
            ClinicId      = clinicId,
            BranchId      = request.BranchId,
            PatientId     = request.PatientId,
            DoctorInfoId  = request.DoctorInfoId,
            VisitTypeId   = request.VisitTypeId,
            Date          = request.Date,
            Type          = request.Type,
            ScheduledTime = request.Type == AppointmentType.Time ? request.ScheduledTime : null,
            Status        = AppointmentStatus.Pending,
        };

        // Auto-assign queue number for queue-based using atomic counter
        if (request.Type == AppointmentType.Queue)
            appointment.QueueNumber = await _uow.QueueCounters.NextAsync(
                request.DoctorInfoId, request.Date, ct);

        appointment.ApplyPrice(visitType.Price, request.DiscountPercent);

        await _uow.Appointments.AddAsync(appointment, ct);
        await _uow.SaveChangesAsync(ct);

        return Result.Success(appointment.Id);
    }
}
