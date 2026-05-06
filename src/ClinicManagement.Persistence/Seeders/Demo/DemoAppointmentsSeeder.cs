using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Seeds appointments relative to UtcNow so the delay-dialog test always works:
///
///   Doctor 1 (Time-based):
///     - Works from (UtcNow - 2h) to (UtcNow + 6h)
///     - Today's appointments start at (UtcNow - 1h 30m), every 30 min
///     - First 3 slots are Pending and in the past → perfect for AutoShift / MarkMissed
///
///   Doctor 2 (Queue-based):
///     - Works from (UtcNow - 2h) to (UtcNow + 6h)
///     - Today's queue: mix of Completed, InProgress, Waiting, Pending
///     - No delay dialog (queue type)
///
///   Doctor 3 (Time-based):
///     - Works from (UtcNow + 1h) to (UtcNow + 9h) — hasn't started yet
///     - Today's appointments all in the future → all Pending
///     - Check-in will NOT trigger delay dialog (on time)
///
/// Past 30 days: realistic historical data for all 3 doctors.
/// </summary>
public class DemoAppointmentsSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DemoAppointmentsSeeder> _logger;

    public DemoAppointmentsSeeder(ApplicationDbContext db, ILogger<DemoAppointmentsSeeder> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task SeedAsync(DemoClinicContext ctx)
    {
        var existing = await _db.Set<Appointment>().IgnoreQueryFilters()
            .CountAsync(a => a.ClinicId == ctx.ClinicId);

        if (existing >= 50) { _logger.LogInformation("Appointments already seeded — skipping"); return; }

        var patients = await _db.Set<Patient>().IgnoreQueryFilters()
            .Where(p => p.ClinicId == ctx.ClinicId)
            .Select(p => p.Id)
            .ToListAsync();

        if (patients.Count == 0) { _logger.LogWarning("No patients — skipping appointments"); return; }

        var now   = DateTimeOffset.UtcNow;
        // Use local date and local time throughout so seeded appointments match
        // what the user sees in the UI and what MarkMissed compares against.
        var today = DateOnly.FromDateTime(now.LocalDateTime);
        var all   = new List<Appointment>();

        all.AddRange(SeedDoctor1(ctx, patients, today, now));
        all.AddRange(SeedDoctor2(ctx, patients, today, now));
        all.AddRange(SeedDoctor3(ctx, patients, today, now));

        _db.Set<Appointment>().AddRange(all);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} demo appointments across 3 doctors", all.Count);
    }

    // ── Doctor 1: Time-based — starts 2h ago, appointments from 1h30m ago ────
    // Purpose: check in NOW → doctor is 2h late → dialog appears
    //   AutoShift: shifts the 3 past-Pending slots forward by 2h
    //   MarkMissed: marks those 3 past-Pending slots as NoShow

    private static List<Appointment> SeedDoctor1(
        DemoClinicContext ctx, List<Guid> patients, DateOnly today, DateTimeOffset now)
    {
        var list = new List<Appointment>();

        // ── Past 30 days ──────────────────────────────────────────────────────
        var pastStatuses = new[]
        {
            AppointmentStatus.Completed, AppointmentStatus.Completed, AppointmentStatus.Completed,
            AppointmentStatus.Completed, AppointmentStatus.Cancelled, AppointmentStatus.NoShow,
        };

        for (int day = 1; day <= 30; day++)
        {
            var date  = today.AddDays(-day);
            var count = day % 3 == 0 ? 4 : 3;
            for (int i = 0; i < count; i++)
            {
                // Use fixed clock times for historical data — doesn't matter for testing
                var time  = new TimeOnly(9 + i, 0);
                var vtId  = i % 2 == 0 ? ctx.VisitTypeId : ctx.VisitType2Id;
                var price = vtId == ctx.VisitTypeId ? 150m : 80m;
                list.Add(Make(ctx.ClinicId, ctx.BranchId,
                    patients[(day * 2 + i) % patients.Count],
                    ctx.DoctorInfoId, vtId, date, AppointmentType.Time,
                    pastStatuses[(day + i) % pastStatuses.Length],
                    time, time.AddMinutes(30), null, price,
                    now.AddDays(-day), ctx.OwnerUserId));
            }
        }

        // ── Today ─────────────────────────────────────────────────────────────
        // Slots relative to UtcNow so they're always testable:
        //   now-1h30m  Pending  ← in the past → MarkMissed will catch this
        //   now-1h00m  Pending  ← in the past → MarkMissed will catch this
        //   now-0h30m  Pending  ← in the past → MarkMissed will catch this
        //   now+0h00m  Pending  ← right now
        //   now+0h30m  Pending
        //   now+1h00m  Pending
        //   now+1h30m  Pending
        //   now+2h00m  Pending
        //   now+2h30m  Pending
        //   now+3h00m  Pending

        var offsets = new[]
        {
            -90, -60, -30,   // past — these are the ones delay dialog acts on
              0,  30,  60,
             90, 120, 150, 180,
        };

        for (int i = 0; i < offsets.Length; i++)
        {
            // Use local time so display and MarkMissed comparison are consistent
            var slotLocal = now.ToLocalTime().AddMinutes(offsets[i]);
            var slotTime  = TimeOnly.FromDateTime(slotLocal.LocalDateTime);
            var vtId      = i % 2 == 0 ? ctx.VisitTypeId : ctx.VisitType2Id;
            var price     = vtId == ctx.VisitTypeId ? 150m : 80m;
            list.Add(Make(ctx.ClinicId, ctx.BranchId,
                patients[i % patients.Count],
                ctx.DoctorInfoId, vtId, today, AppointmentType.Time,
                AppointmentStatus.Pending,
                slotTime, slotTime.AddMinutes(25), null, price,
                now.AddHours(-3), ctx.OwnerUserId));
        }

        return list;
    }

    // ── Doctor 2: Queue-based — starts 2h ago ────────────────────────────────
    // No delay dialog for queue doctors. Just realistic queue data.

    private static List<Appointment> SeedDoctor2(
        DemoClinicContext ctx, List<Guid> patients, DateOnly today, DateTimeOffset now)
    {
        var list = new List<Appointment>();

        // Past 30 days
        var pastStatuses = new[]
        {
            AppointmentStatus.Completed, AppointmentStatus.Completed, AppointmentStatus.Completed,
            AppointmentStatus.Cancelled, AppointmentStatus.NoShow,
        };

        for (int day = 1; day <= 30; day++)
        {
            var date  = today.AddDays(-day);
            var count = day % 2 == 0 ? 5 : 4;
            for (int i = 0; i < count; i++)
            {
                var vtId  = i % 2 == 0 ? ctx.VisitType3Id : ctx.VisitType4Id;
                var price = vtId == ctx.VisitType3Id ? 100m : 50m;
                list.Add(Make(ctx.ClinicId, ctx.BranchId,
                    patients[(day * 3 + i + 10) % patients.Count],
                    ctx.Doctor2InfoId, vtId, date, AppointmentType.Queue,
                    pastStatuses[(day + i) % pastStatuses.Length],
                    null, null, i + 1, price,
                    now.AddDays(-day), ctx.OwnerUserId));
            }
        }

        // Today — realistic queue progression
        var todayQueue = new[]
        {
            (1,  AppointmentStatus.Completed,  ctx.VisitType3Id, 100m),
            (2,  AppointmentStatus.Completed,  ctx.VisitType4Id,  50m),
            (3,  AppointmentStatus.Completed,  ctx.VisitType3Id, 100m),
            (4,  AppointmentStatus.InProgress, ctx.VisitType3Id, 100m),
            (5,  AppointmentStatus.Waiting,    ctx.VisitType4Id,  50m),
            (6,  AppointmentStatus.Waiting,    ctx.VisitType3Id, 100m),
            (7,  AppointmentStatus.Pending,    ctx.VisitType4Id,  50m),
            (8,  AppointmentStatus.Pending,    ctx.VisitType3Id, 100m),
            (9,  AppointmentStatus.Pending,    ctx.VisitType4Id,  50m),
            (10, AppointmentStatus.Pending,    ctx.VisitType3Id, 100m),
        };

        for (int i = 0; i < todayQueue.Length; i++)
        {
            var (qNum, status, vtId, price) = todayQueue[i];
            list.Add(Make(ctx.ClinicId, ctx.BranchId,
                patients[(i + 5) % patients.Count],
                ctx.Doctor2InfoId, vtId, today, AppointmentType.Queue,
                status, null, null, qNum, price,
                now.AddHours(-3), ctx.OwnerUserId));
        }

        return list;
    }

    // ── Doctor 3: Time-based — Downtown Branch, starts 1h from now ──────────
    // Check-in will NOT trigger delay dialog (doctor is on time / early).
    // All today's appointments are in the future.
    // Assigned to Branch2 — tests the branch filter in the toolbar.

    private static List<Appointment> SeedDoctor3(
        DemoClinicContext ctx, List<Guid> patients, DateOnly today, DateTimeOffset now)
    {
        var list = new List<Appointment>();

        // Past 30 days
        var pastStatuses = new[]
        {
            AppointmentStatus.Completed, AppointmentStatus.Completed,
            AppointmentStatus.Completed, AppointmentStatus.Cancelled,
        };

        for (int day = 1; day <= 30; day++)
        {
            var date  = today.AddDays(-day);
            var count = day % 3 == 0 ? 4 : 3;
            for (int i = 0; i < count; i++)
            {
                var time  = new TimeOnly(14 + i, 0);
                var vtId  = i % 2 == 0 ? ctx.VisitType5Id : ctx.VisitType6Id;
                var price = vtId == ctx.VisitType5Id ? 200m : 120m;
                list.Add(Make(ctx.ClinicId, ctx.Branch2Id,
                    patients[(day * 4 + i + 20) % patients.Count],
                    ctx.Doctor3InfoId, vtId, date, AppointmentType.Time,
                    pastStatuses[(day + i) % pastStatuses.Length],
                    time, time.AddMinutes(30), null, price,
                    now.AddDays(-day), ctx.OwnerUserId));
            }
        }

        // Today — all future slots (doctor hasn't started yet)
        var futureOffsets = new[] { 60, 90, 120, 150, 180, 210, 240, 270 };

        for (int i = 0; i < futureOffsets.Length; i++)
        {
            var slotLocal = now.ToLocalTime().AddMinutes(futureOffsets[i]);
            var slotTime  = TimeOnly.FromDateTime(slotLocal.LocalDateTime);
            var vtId      = i % 2 == 0 ? ctx.VisitType5Id : ctx.VisitType6Id;
            var price     = vtId == ctx.VisitType5Id ? 200m : 120m;
            list.Add(Make(ctx.ClinicId, ctx.Branch2Id,
                patients[(i + 15) % patients.Count],
                ctx.Doctor3InfoId, vtId, today, AppointmentType.Time,
                AppointmentStatus.Pending,
                slotTime, slotTime.AddMinutes(30), null, price,
                now.AddHours(-1), ctx.OwnerUserId));
        }

        return list;
    }

    // ── Factory ───────────────────────────────────────────────────────────────

    private static Appointment Make(
        Guid clinicId, Guid branchId, Guid patientId, Guid doctorInfoId, Guid visitTypeId,
        DateOnly date, AppointmentType type, AppointmentStatus status,
        TimeOnly? scheduledTime, TimeOnly? endTime, int? queueNumber,
        decimal price, DateTimeOffset createdAt, Guid createdBy)
    {
        var appt = new Appointment
        {
            ClinicId      = clinicId,
            BranchId      = branchId,
            PatientId     = patientId,
            DoctorInfoId  = doctorInfoId,
            VisitTypeId   = visitTypeId,
            Date          = date,
            Type          = type,
            Status        = status,
            ScheduledTime = scheduledTime,
            EndTime       = endTime,
            QueueNumber   = queueNumber,
            CreatedAt     = createdAt,
            UpdatedAt     = createdAt,
            CreatedBy     = createdBy,
            UpdatedBy     = createdBy,
        };
        appt.ApplyPrice(price);
        return appt;
    }
}
