using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public class CreateAppointmentHandler : IRequestHandler<CreateAppointmentCommand, Result<Guid>>
{
    private readonly IAppointmentRepository  _appointments;
    private readonly IDoctorScheduleRepository _schedules;
    private readonly IQueueCounterRepository _queueCounters;
    private readonly IUnitOfWork            _uow;
    private readonly ICurrentUserService    _currentUser;

    public CreateAppointmentHandler(
        IAppointmentRepository appointments,
        IDoctorScheduleRepository schedules,
        IQueueCounterRepository queueCounters,
        IUnitOfWork uow,
        ICurrentUserService currentUser)
    {
        _appointments  = appointments;
        _schedules     = schedules;
        _queueCounters = queueCounters;
        _uow           = uow;
        _currentUser   = currentUser;
    }

    public async Task<Result<Guid>> Handle(CreateAppointmentCommand request, CancellationToken ct)
    {
        var clinicId = _currentUser.GetRequiredClinicId();

        var visitType = await _schedules.GetVisitTypeByIdAsync(request.VisitTypeId, ct);
        if (visitType is null || !visitType.IsActive)
            return Result.Failure<Guid>(ErrorCodes.NOT_FOUND, "Visit type not found or inactive");

        if (request.Type == AppointmentType.Time)
        {
            if (request.ScheduledTime is null)
                return Result.Failure<Guid>(ErrorCodes.VALIDATION_ERROR, "Scheduled time is required");

            var taken = await _appointments.TimeSlotTakenAsync(
                request.DoctorInfoId, request.Date, request.ScheduledTime.Value, null, ct);
            if (taken)
                return Result.Failure<Guid>(ErrorCodes.CONFLICT, "This time slot is already booked");
        }

        var defaultDuration = visitType.Schedule?.DoctorInfo?.DefaultVisitDurationMinutes;
        var visitDuration   = request.VisitDurationMinutes ?? defaultDuration;

        var appointment = Domain.Entities.Appointment.Create(
            clinicId:             clinicId,
            branchId:             request.BranchId,
            patientId:            request.PatientId,
            doctorInfoId:         request.DoctorInfoId,
            visitTypeId:          request.VisitTypeId,
            date:                 request.Date,
            type:                 request.Type,
            scheduledTime:        request.ScheduledTime,
            visitDurationMinutes: visitDuration,
            price:                visitType.Price,
            discountPercent:      request.DiscountPercent);

        if (request.Type == AppointmentType.Queue)
            appointment.QueueNumber = await _queueCounters.NextAsync(request.DoctorInfoId, request.Date, ct);

        await _appointments.AddAsync(appointment, ct);

        if (request.MarkAsPaid)
            appointment.InvoiceId = Guid.NewGuid();

        await _uow.SaveChangesAsync(ct);

        return Result.Success(appointment.Id);
    }
}
