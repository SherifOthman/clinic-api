using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using MediatR;

namespace ClinicManagement.Application.Features.Appointments.Commands;

public class RescheduleDoctorHandler : IRequestHandler<RescheduleDoctorCommand, Result<int>>
{
    private readonly IUnitOfWork _uow;

    public RescheduleDoctorHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<int>> Handle(RescheduleDoctorCommand request, CancellationToken ct)
    {
        var schedule = await _uow.DoctorSchedules.GetScheduleAsync(request.DoctorInfoId, request.BranchId, ct);
        if (schedule is null)
            return Result.Failure<int>(ErrorCodes.NOT_FOUND, "Doctor schedule not found");

        var workingDays = schedule.WorkingDays
            .Where(w => w.IsAvailable)
            .Select(w => w.Day)
            .ToHashSet();

        if (workingDays.Count == 0)
            return Result.Failure<int>(ErrorCodes.VALIDATION_ERROR, "Doctor has no working days configured");

        // Load all pending/waiting appointments from the given date onwards
        var appointments = await _uow.Appointments.GetFutureByDoctorForUpdateAsync(
            request.DoctorInfoId, request.FromDate, ct);

        if (appointments.Count == 0)
            return Result.Success(0);

        // Group by original date — each date's appointments move to the next available working day
        var byDate = appointments
            .GroupBy(a => a.Date)
            .OrderBy(g => g.Key)
            .ToList();

        // Track which target dates have already received carry-over patients in this run,
        // so we can correctly stack multiple groups onto the same target day if needed.
        // Key = target date, Value = how many carry-over slots are already reserved at the front.
        var carryOverCountByDate = new Dictionary<DateOnly, int>();

        // Dates that already have appointments (before this reschedule run)
        // — used to find the next available working day.
        // We allow landing on dates that already have appointments (we just push them back).
        var originalDates = appointments.Select(a => a.Date).ToHashSet();

        int rescheduled = 0;

        foreach (var group in byDate)
        {
            var originalDate = group.Key;
            var groupAppts   = group.OrderBy(a => a.QueueNumber ?? 0).ToList();

            // Find the next working day after the original date.
            // Unlike before, we DO allow landing on dates that already have appointments —
            // the carry-overs go first, existing patients shift back.
            var newDate = FindNextWorkingDay(originalDate, workingDays);
            if (newDate is null) continue;

            // How many carry-over slots are already at the front of this target date?
            carryOverCountByDate.TryGetValue(newDate.Value, out int alreadyCarriedOver);

            // Load existing pending/waiting appointments on the target date
            // (only if this is the first group landing on this date)
            List<Appointment> existingOnTarget = new();
            if (alreadyCarriedOver == 0)
            {
                existingOnTarget = await _uow.Appointments.GetByDoctorDatePendingForUpdateAsync(
                    request.DoctorInfoId, newDate.Value, ct);
            }

            // Assign carry-over patients queue numbers starting from (alreadyCarriedOver + 1)
            // so they all go before the existing patients.
            int nextCarryOverSlot = alreadyCarriedOver + 1;

            foreach (var appt in groupAppts)
            {
                appt.Date        = newDate.Value;
                appt.QueueNumber = nextCarryOverSlot++;

                if (appt.Status == Domain.Enums.AppointmentStatus.Waiting)
                    appt.Status = Domain.Enums.AppointmentStatus.Pending;

                rescheduled++;
            }

            // Push existing patients on the target date back by the number of carry-overs added
            int totalCarryOvers = nextCarryOverSlot - 1; // = alreadyCarriedOver + groupAppts.Count
            if (existingOnTarget.Count > 0)
            {
                int offset = groupAppts.Count; // how many new carry-overs we just added
                foreach (var e in existingOnTarget.OrderBy(e => e.QueueNumber ?? 0))
                    e.QueueNumber = (e.QueueNumber ?? 0) + offset;
            }

            carryOverCountByDate[newDate.Value] = totalCarryOvers;
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success(rescheduled);
    }

    /// <summary>
    /// Finds the next working day after the given date within 60 days.
    /// Unlike the previous version, we allow landing on dates that already have appointments —
    /// carry-overs go first, existing patients shift back.
    /// </summary>
    private static DateOnly? FindNextWorkingDay(DateOnly afterDate, HashSet<DayOfWeek> workingDays)
    {
        var candidate = afterDate.AddDays(1);
        var limit     = afterDate.AddDays(60);

        while (candidate <= limit)
        {
            if (workingDays.Contains(candidate.DayOfWeek))
                return candidate;
            candidate = candidate.AddDays(1);
        }

        return null;
    }
}
