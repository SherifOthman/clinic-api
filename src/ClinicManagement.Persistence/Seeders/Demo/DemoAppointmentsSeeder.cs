using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Seeds appointments for all 3 demo doctors:
///   Doctor 1 — Time-based, 9am–5pm  (past 30 days + today)
///   Doctor 2 — Queue-based, 8am–2pm (past 30 days + today)
///   Doctor 3 — Time-based, 2pm–8pm  (past 30 days + today)
///
/// Total: ~120 appointments — enough to test multi-doctor view, pagination,
/// and both appointment types side-by-side.
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
        var today = DateOnly.FromDateTime(now.Date);
        var all   = new List<Appointment>();

        all.AddRange(SeedDoctor1(ctx, patients, today, now));  // Time-based
        all.AddRange(SeedDoctor2(ctx, patients, today, now));  // Queue-based
        all.AddRange(SeedDoctor3(ctx, patients, today, now));  // Time-based (afternoon)

        _db.Set<Appointment>().AddRange(all);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} demo appointments across 3 doctors", all.Count);
    }

    // ── Doctor 1: Time-based, 9am–5pm ────────────────────────────────────────

    private static List<Appointment> SeedDoctor1(
        DemoClinicContext ctx, List<Guid> patients, DateOnly today, DateTimeOffset now)
    {
        var list = new List<Appointment>();
        var pastStatuses = new[]
        {
            AppointmentStatus.Completed, AppointmentStatus.Completed, AppointmentStatus.Completed,
            AppointmentStatus.Completed, AppointmentStatus.Cancelled, AppointmentStatus.NoShow,
        };
        var times = new[] { new TimeOnly(9,0), new TimeOnly(9,30), new TimeOnly(10,0), new TimeOnly(10,30),
                            new TimeOnly(11,0), new TimeOnly(11,30), new TimeOnly(14,0), new TimeOnly(14,30) };

        // Past 30 days
        for (int day = 1; day <= 30; day++)
        {
            var date  = today.AddDays(-day);
            var count = day % 3 == 0 ? 3 : 2;
            for (int i = 0; i < count; i++)
            {
                var time  = times[(day + i) % times.Length];
                var vtId  = i % 2 == 0 ? ctx.VisitTypeId : ctx.VisitType2Id;
                var price = vtId == ctx.VisitTypeId ? 150m : 80m;
                var appt  = Make(ctx.ClinicId, ctx.BranchId, patients[(day * 2 + i) % patients.Count],
                    ctx.DoctorInfoId, vtId, date, AppointmentType.Time,
                    pastStatuses[(day + i) % pastStatuses.Length],
                    time, time.AddMinutes(20), null, price, now.AddDays(-day), ctx.OwnerUserId);
                list.Add(appt);
            }
        }

        // Today — mix of statuses including PAST-TIME Pending slots for delay dialog testing
        // Doctor 1 starts at 9am. If they check in late (e.g. at 11am), the delay dialog
        // will offer to shift/mark-missed the Pending appointments at 9:00 and 9:30.
        var todaySlots = new[]
        {
            // Past slots — Pending (these trigger the delay dialog options)
            (new TimeOnly(9,  0), AppointmentStatus.Pending,    ctx.VisitTypeId,  150m),
            (new TimeOnly(9, 30), AppointmentStatus.Pending,    ctx.VisitType2Id,  80m),
            // Current/near-future
            (new TimeOnly(10, 0), AppointmentStatus.Waiting,    ctx.VisitTypeId,  150m),
            (new TimeOnly(10,30), AppointmentStatus.Waiting,    ctx.VisitType2Id,  80m),
            (new TimeOnly(11, 0), AppointmentStatus.Pending,    ctx.VisitTypeId,  150m),
            (new TimeOnly(11,30), AppointmentStatus.Pending,    ctx.VisitType2Id,  80m),
            (new TimeOnly(14, 0), AppointmentStatus.Pending,    ctx.VisitType2Id,  80m),
            (new TimeOnly(14,30), AppointmentStatus.Pending,    ctx.VisitTypeId,  150m),
            (new TimeOnly(15, 0), AppointmentStatus.Pending,    ctx.VisitType2Id,  80m),
            (new TimeOnly(15,30), AppointmentStatus.Pending,    ctx.VisitTypeId,  150m),
        };

        for (int i = 0; i < todaySlots.Length; i++)
        {
            var (time, status, vtId, price) = todaySlots[i];
            list.Add(Make(ctx.ClinicId, ctx.BranchId, patients[i % patients.Count],
                ctx.DoctorInfoId, vtId, today, AppointmentType.Time,
                status, time, time.AddMinutes(20), null, price, now.AddHours(-2), ctx.OwnerUserId));
        }

        return list;
    }

    // ── Doctor 2: Queue-based, 8am–2pm ───────────────────────────────────────

    private static List<Appointment> SeedDoctor2(
        DemoClinicContext ctx, List<Guid> patients, DateOnly today, DateTimeOffset now)
    {
        var list = new List<Appointment>();
        var pastStatuses = new[]
        {
            AppointmentStatus.Completed, AppointmentStatus.Completed, AppointmentStatus.Completed,
            AppointmentStatus.Cancelled, AppointmentStatus.NoShow,
        };

        // Past 30 days — queue appointments (no scheduled time, just queue numbers)
        for (int day = 1; day <= 30; day++)
        {
            var date  = today.AddDays(-day);
            var count = day % 2 == 0 ? 4 : 3;
            for (int i = 0; i < count; i++)
            {
                var vtId  = i % 2 == 0 ? ctx.VisitType3Id : ctx.VisitType4Id;
                var price = vtId == ctx.VisitType3Id ? 100m : 50m;
                var appt  = Make(ctx.ClinicId, ctx.BranchId, patients[(day * 3 + i + 10) % patients.Count],
                    ctx.Doctor2InfoId, vtId, date, AppointmentType.Queue,
                    pastStatuses[(day + i) % pastStatuses.Length],
                    null, null, i + 1, price, now.AddDays(-day), ctx.OwnerUserId);
                list.Add(appt);
            }
        }

        // Today — queue with all statuses
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
            (11, AppointmentStatus.Pending,    ctx.VisitType4Id,  50m),
            (12, AppointmentStatus.Pending,    ctx.VisitType3Id, 100m),
        };

        for (int i = 0; i < todayQueue.Length; i++)
        {
            var (qNum, status, vtId, price) = todayQueue[i];
            list.Add(Make(ctx.ClinicId, ctx.BranchId, patients[(i + 5) % patients.Count],
                ctx.Doctor2InfoId, vtId, today, AppointmentType.Queue,
                status, null, null, qNum, price, now.AddHours(-3), ctx.OwnerUserId));
        }

        return list;
    }

    // ── Doctor 3: Time-based, 2pm–8pm ────────────────────────────────────────

    private static List<Appointment> SeedDoctor3(
        DemoClinicContext ctx, List<Guid> patients, DateOnly today, DateTimeOffset now)
    {
        var list = new List<Appointment>();
        var pastStatuses = new[]
        {
            AppointmentStatus.Completed, AppointmentStatus.Completed,
            AppointmentStatus.Completed, AppointmentStatus.Cancelled,
        };
        var times = new[] { new TimeOnly(14,0), new TimeOnly(14,30), new TimeOnly(15,0), new TimeOnly(15,30),
                            new TimeOnly(16,0), new TimeOnly(16,30), new TimeOnly(17,0), new TimeOnly(17,30) };

        // Past 30 days
        for (int day = 1; day <= 30; day++)
        {
            var date  = today.AddDays(-day);
            var count = day % 3 == 0 ? 3 : 2;
            for (int i = 0; i < count; i++)
            {
                var time  = times[(day + i) % times.Length];
                var vtId  = i % 2 == 0 ? ctx.VisitType5Id : ctx.VisitType6Id;
                var price = vtId == ctx.VisitType5Id ? 200m : 120m;
                var appt  = Make(ctx.ClinicId, ctx.BranchId, patients[(day * 4 + i + 20) % patients.Count],
                    ctx.Doctor3InfoId, vtId, date, AppointmentType.Time,
                    pastStatuses[(day + i) % pastStatuses.Length],
                    time, time.AddMinutes(30), null, price, now.AddDays(-day), ctx.OwnerUserId);
                list.Add(appt);
            }
        }

        // Today — afternoon slots
        var todaySlots = new[]
        {
            (new TimeOnly(14, 0), AppointmentStatus.Completed,  ctx.VisitType5Id, 200m),
            (new TimeOnly(14,30), AppointmentStatus.Completed,  ctx.VisitType6Id, 120m),
            (new TimeOnly(15, 0), AppointmentStatus.InProgress, ctx.VisitType5Id, 200m),
            (new TimeOnly(15,30), AppointmentStatus.Waiting,    ctx.VisitType6Id, 120m),
            (new TimeOnly(16, 0), AppointmentStatus.Waiting,    ctx.VisitType5Id, 200m),
            (new TimeOnly(16,30), AppointmentStatus.Pending,    ctx.VisitType6Id, 120m),
            (new TimeOnly(17, 0), AppointmentStatus.Pending,    ctx.VisitType5Id, 200m),
            (new TimeOnly(17,30), AppointmentStatus.Pending,    ctx.VisitType6Id, 120m),
        };

        for (int i = 0; i < todaySlots.Length; i++)
        {
            var (time, status, vtId, price) = todaySlots[i];
            list.Add(Make(ctx.ClinicId, ctx.BranchId, patients[(i + 15) % patients.Count],
                ctx.Doctor3InfoId, vtId, today, AppointmentType.Time,
                status, time, time.AddMinutes(30), null, price, now.AddHours(-1), ctx.OwnerUserId));
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
