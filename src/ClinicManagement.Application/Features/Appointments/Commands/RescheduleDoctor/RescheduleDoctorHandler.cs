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
        // Load the doctor's working days for this branch
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

        // Group by original date — each date's appointments move to the next available day
        var byDate = appointments
            .GroupBy(a => a.Date)
            .OrderBy(g => g.Key)
            .ToList();

        // Track which dates are already "used" by rescheduled appointments
        // so we don't pile multiple groups onto the same day
        var usedDates = new HashSet<DateOnly>();

        // Pre-load existing appointment dates for this doctor to avoid conflicts
        // (dates that already have appointments shouldn't receive more)
        var existingDates = appointments
            .Select(a => a.Date)
            .ToHashSet();

        int rescheduled = 0;

        foreach (var group in byDate)
        {
            var originalDate = group.Key;
            var groupAppts   = group.OrderBy(a => a.QueueNumber ?? 0)
                                    .ThenBy(a => a.ScheduledTime ?? TimeOnly.MinValue)
                                    .ToList();

            // Find the next available working day after the original date
            var newDate = FindNextWorkingDay(originalDate, workingDays, usedDates, existingDates);

            if (newDate is null)
            {
                // No available day found within 60 days — skip this group
                continue;
            }

            usedDates.Add(newDate.Value);

            // For queue-based: reassign queue numbers starting from 1 on the new date
            // (or continue from existing queue numbers on that date if any)
            int queueOffset = 0;

            foreach (var appt in groupAppts)
            {
                appt.Date = newDate.Value;

                if (appt.Type == Domain.Enums.AppointmentType.Queue)
                {
                    // Keep relative queue order; actual numbers will be sequential
                    appt.QueueNumber = (appt.QueueNumber ?? 0) + queueOffset;
                }
                // Time-based: keep the same time slot on the new date
                // (the receptionist can adjust individual slots if needed)

                rescheduled++;
            }
        }

        await _uow.SaveChangesAsync(ct);
        return Result.Success(rescheduled);
    }

    /// <summary>
    /// Finds the next working day after the given date that isn't already used
    /// or occupied by existing appointments. Searches up to 60 days ahead.
    /// </summary>
    private static DateOnly? FindNextWorkingDay(
        DateOnly afterDate,
        HashSet<DayOfWeek> workingDays,
        HashSet<DateOnly> usedDates,
        HashSet<DateOnly> existingDates)
    {
        var candidate = afterDate.AddDays(1);
        var limit     = afterDate.AddDays(60);

        while (candidate <= limit)
        {
            if (workingDays.Contains(candidate.DayOfWeek)
                && !usedDates.Contains(candidate)
                && !existingDates.Contains(candidate))
            {
                return candidate;
            }
            candidate = candidate.AddDays(1);
        }

        return null;
    }
}
