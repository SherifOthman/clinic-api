using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
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

        var visitType = await _uow.DoctorSchedules.GetVisitTypeByIdAsync(request.VisitTypeId, ct);
        if (visitType is null || !visitType.IsActive)
            return Result.Failure<Guid>(ErrorCodes.NOT_FOUND, "Visit type not found or inactive");

        if (request.Type == AppointmentType.Time)
        {
            if (request.ScheduledTime is null)
                return Result.Failure<Guid>(ErrorCodes.VALIDATION_ERROR, "Scheduled time is required");

            var taken = await _uow.Appointments.TimeSlotTakenAsync(
                request.DoctorInfoId, request.Date, request.ScheduledTime.Value, null, ct);
            if (taken)
                return Result.Failure<Guid>(ErrorCodes.CONFLICT, "This time slot is already booked");
        }

        // Resolve default visit duration from the doctor's schedule when not overridden
        var defaultDuration = visitType.Schedule?.DoctorInfo?.DefaultVisitDurationMinutes;
        var visitDuration   = request.VisitDurationMinutes ?? defaultDuration;

        // Appointment.Create() owns all construction logic — no inline property assignments here
        var appointment = Domain.Entities.Appointment.Create(
            clinicId:            clinicId,
            branchId:            request.BranchId,
            patientId:           request.PatientId,
            doctorInfoId:        request.DoctorInfoId,
            visitTypeId:         request.VisitTypeId,
            date:                request.Date,
            type:                request.Type,
            scheduledTime:       request.ScheduledTime,
            visitDurationMinutes: visitDuration,
            price:               visitType.Price,
            discountPercent:     request.DiscountPercent);

        if (request.Type == AppointmentType.Queue)
            appointment.QueueNumber = await _uow.QueueCounters.NextAsync(
                request.DoctorInfoId, request.Date, ct);

        await _uow.Appointments.AddAsync(appointment, ct);
        await _uow.SaveChangesAsync(ct);

        return Result.Success(appointment.Id);
    }
}
