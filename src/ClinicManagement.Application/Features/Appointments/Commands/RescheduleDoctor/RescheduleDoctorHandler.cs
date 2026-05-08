using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public class RescheduleDoctorHandler : IRequestHandler<RescheduleDoctorCommand, Result<int>>
{
    private readonly IDoctorScheduleRepository _schedules;
    private readonly IAppointmentRepository    _appointments;
    private readonly IUnitOfWork _uow;

    public RescheduleDoctorHandler(
        IDoctorScheduleRepository schedules,
        IAppointmentRepository appointments,
        IUnitOfWork uow)
    {
        _schedules    = schedules;
        _appointments = appointments;
        _uow          = uow;
    }

    public async Task<Result<int>> Handle(RescheduleDoctorCommand request, CancellationToken ct)
    {
        var schedule = await _schedules.GetScheduleAsync(request.DoctorInfoId, request.BranchId, ct);
        if (schedule is null)
            return Result.Failure<int>(ErrorCodes.NOT_FOUND, "Doctor schedule not found");

        if (schedule.WorkingDays.Count == 0 || !schedule.WorkingDays.Any(w => w.IsAvailable))
            return Result.Failure<int>(ErrorCodes.VALIDATION_ERROR, "Doctor has no working days configured");

        var appointments = await _appointments.GetFutureByDoctorForUpdateAsync(
            request.DoctorInfoId, request.FromDate, ct);

        if (appointments.Count == 0)
            return Result.Success(0);

        var byDate = appointments
            .GroupBy(a => a.Date)
            .OrderBy(g => g.Key)
            .ToList();

        var carryOverCountByDate = new Dictionary<DateOnly, int>();
        int rescheduled = 0;

        foreach (var group in byDate)
        {
            var originalDate = group.Key;
            var groupAppts   = group.OrderBy(a => a.QueueNumber ?? 0).ToList();

            // FindNextWorkingDay now lives on the domain entity
            var newDate = schedule.FindNextWorkingDay(originalDate);
            if (newDate is null) continue;

            carryOverCountByDate.TryGetValue(newDate.Value, out int alreadyCarriedOver);

            List<Appointment> existingOnTarget = new();
            if (alreadyCarriedOver == 0)
            {
                existingOnTarget = await _appointments.GetByDoctorDatePendingForUpdateAsync(
                    request.DoctorInfoId, newDate.Value, ct);
            }

            int nextCarryOverSlot = alreadyCarriedOver + 1;

            foreach (var appt in groupAppts)
            {
                appt.Date        = newDate.Value;
                appt.QueueNumber = nextCarryOverSlot++;

                if (appt.Status == Domain.Enums.AppointmentStatus.Waiting)
                    appt.Status = Domain.Enums.AppointmentStatus.Pending;

                rescheduled++;
            }

            int totalCarryOvers = nextCarryOverSlot - 1;
            if (existingOnTarget.Count > 0)
            {
                int offset = groupAppts.Count;
                foreach (var e in existingOnTarget.OrderBy(e => e.QueueNumber ?? 0))
                    e.QueueNumber = (e.QueueNumber ?? 0) + offset;
            }

            carryOverCountByDate[newDate.Value] = totalCarryOvers;
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success(rescheduled);
    }
}
