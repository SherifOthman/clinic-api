using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Seeds queue-based appointments for all 3 demo doctors.
/// All doctors use Queue type — Time-based is kept in the backend for future use.
///
///   Doctor 1 (Main Branch) — Queue, starts 00:01 local → always late on check-in
///   Doctor 2 (Main Branch) — Queue, starts 00:01 local → always late on check-in
///   Doctor 3 (Downtown Branch) — Queue, starts 23:00 local → always on time
///
/// Today's appointments are seeded with realistic queue progressions.
/// Past 30 days: historical data for all 3 doctors.
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
        var today = DateOnly.FromDateTime(now.LocalDateTime);
        var all   = new List<Appointment>();

        all.AddRange(SeedDoctor1(ctx, patients, today, now));
        all.AddRange(SeedDoctor2(ctx, patients, today, now));
        all.AddRange(SeedDoctor3(ctx, patients, today, now));

        _db.Set<Appointment>().AddRange(all);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} demo appointments across 3 doctors", all.Count);
    }

    // ── Doctor 1: Queue-based, Main Branch ───────────────────────────────────
    // Starts 00:01 local → checking in at any time = always late → delay dialog
    // Today: mix of Completed, InProgress, Waiting, Pending

    private static List<Appointment> SeedDoctor1(
        DemoClinicContext ctx, List<Guid> patients, DateOnly today, DateTimeOffset now)
    {
        var list = new List<Appointment>();
        var pastStatuses = new[]
        {
            AppointmentStatus.Completed, AppointmentStatus.Completed, AppointmentStatus.Completed,
            AppointmentStatus.Completed, AppointmentStatus.Cancelled, AppointmentStatus.NoShow,
        };

        // Past 30 days
        for (int day = 1; day <= 30; day++)
        {
            var date  = today.AddDays(-day);
            var count = day % 3 == 0 ? 5 : 4;
            for (int i = 0; i < count; i++)
            {
                var vtId  = i % 2 == 0 ? ctx.VisitTypeId : ctx.VisitType2Id;
                var price = vtId == ctx.VisitTypeId ? 150m : 80m;
                list.Add(Queue(ctx.ClinicId, ctx.BranchId,
                    patients[(day * 2 + i) % patients.Count],
                    ctx.DoctorInfoId, vtId, date,
                    pastStatuses[(day + i) % pastStatuses.Length],
                    i + 1, price, now.AddDays(-day), ctx.OwnerUserId));
            }
        }

        // Today — realistic queue: some done, one in progress, some waiting, rest pending
        var todayQueue = new[]
        {
            (1,  AppointmentStatus.Completed,  ctx.VisitTypeId,  150m),
            (2,  AppointmentStatus.Completed,  ctx.VisitType2Id,  80m),
            (3,  AppointmentStatus.Completed,  ctx.VisitTypeId,  150m),
            (4,  AppointmentStatus.InProgress, ctx.VisitType2Id,  80m),
            (5,  AppointmentStatus.Waiting,    ctx.VisitTypeId,  150m),
            (6,  AppointmentStatus.Waiting,    ctx.VisitType2Id,  80m),
            (7,  AppointmentStatus.Pending,    ctx.VisitTypeId,  150m),
            (8,  AppointmentStatus.Pending,    ctx.VisitType2Id,  80m),
            (9,  AppointmentStatus.Pending,    ctx.VisitTypeId,  150m),
            (10, AppointmentStatus.Pending,    ctx.VisitType2Id,  80m),
            (11, AppointmentStatus.Pending,    ctx.VisitTypeId,  150m),
            (12, AppointmentStatus.Pending,    ctx.VisitType2Id,  80m),
        };

        for (int i = 0; i < todayQueue.Length; i++)
        {
            var (qNum, status, vtId, price) = todayQueue[i];
            list.Add(Queue(ctx.ClinicId, ctx.BranchId,
                patients[i % patients.Count],
                ctx.DoctorInfoId, vtId, today, status,
                qNum, price, now.AddHours(-3), ctx.OwnerUserId));
        }

        return list;
    }

    // ── Doctor 2: Queue-based, Main Branch ───────────────────────────────────
    // Different specialization (Pediatrics) — tests multi-doctor view

    private static List<Appointment> SeedDoctor2(
        DemoClinicContext ctx, List<Guid> patients, DateOnly today, DateTimeOffset now)
    {
        var list = new List<Appointment>();
        var pastStatuses = new[]
        {
            AppointmentStatus.Completed, AppointmentStatus.Completed, AppointmentStatus.Completed,
            AppointmentStatus.Cancelled, AppointmentStatus.NoShow,
        };

        // Past 30 days
        for (int day = 1; day <= 30; day++)
        {
            var date  = today.AddDays(-day);
            var count = day % 2 == 0 ? 5 : 4;
            for (int i = 0; i < count; i++)
            {
                var vtId  = i % 2 == 0 ? ctx.VisitType3Id : ctx.VisitType4Id;
                var price = vtId == ctx.VisitType3Id ? 100m : 50m;
                list.Add(Queue(ctx.ClinicId, ctx.BranchId,
                    patients[(day * 3 + i + 10) % patients.Count],
                    ctx.Doctor2InfoId, vtId, date,
                    pastStatuses[(day + i) % pastStatuses.Length],
                    i + 1, price, now.AddDays(-day), ctx.OwnerUserId));
            }
        }

        // Today — all pending (doctor hasn't started yet or just started)
        for (int i = 0; i < 8; i++)
        {
            var vtId  = i % 2 == 0 ? ctx.VisitType3Id : ctx.VisitType4Id;
            var price = vtId == ctx.VisitType3Id ? 100m : 50m;
            list.Add(Queue(ctx.ClinicId, ctx.BranchId,
                patients[(i + 5) % patients.Count],
                ctx.Doctor2InfoId, vtId, today, AppointmentStatus.Pending,
                i + 1, price, now.AddHours(-2), ctx.OwnerUserId));
        }

        return list;
    }

    // ── Doctor 3: Queue-based, Downtown Branch ────────────────────────────────
    // Starts 23:00 local → checking in = always on time → no delay dialog
    // Tests the branch filter in the toolbar

    private static List<Appointment> SeedDoctor3(
        DemoClinicContext ctx, List<Guid> patients, DateOnly today, DateTimeOffset now)
    {
        var list = new List<Appointment>();
        var pastStatuses = new[]
        {
            AppointmentStatus.Completed, AppointmentStatus.Completed,
            AppointmentStatus.Completed, AppointmentStatus.Cancelled,
        };

        // Past 30 days
        for (int day = 1; day <= 30; day++)
        {
            var date  = today.AddDays(-day);
            var count = day % 3 == 0 ? 4 : 3;
            for (int i = 0; i < count; i++)
            {
                var vtId  = i % 2 == 0 ? ctx.VisitType5Id : ctx.VisitType6Id;
                var price = vtId == ctx.VisitType5Id ? 200m : 120m;
                list.Add(Queue(ctx.ClinicId, ctx.Branch2Id,
                    patients[(day * 4 + i + 20) % patients.Count],
                    ctx.Doctor3InfoId, vtId, date,
                    pastStatuses[(day + i) % pastStatuses.Length],
                    i + 1, price, now.AddDays(-day), ctx.OwnerUserId));
            }
        }

        // Today — all pending (doctor hasn't started yet)
        for (int i = 0; i < 6; i++)
        {
            var vtId  = i % 2 == 0 ? ctx.VisitType5Id : ctx.VisitType6Id;
            var price = vtId == ctx.VisitType5Id ? 200m : 120m;
            list.Add(Queue(ctx.ClinicId, ctx.Branch2Id,
                patients[(i + 15) % patients.Count],
                ctx.Doctor3InfoId, vtId, today, AppointmentStatus.Pending,
                i + 1, price, now.AddHours(-1), ctx.OwnerUserId));
        }

        return list;
    }

    // ── Factory — queue appointment ───────────────────────────────────────────

    private static Appointment Queue(
        Guid clinicId, Guid branchId, Guid patientId, Guid doctorInfoId, Guid visitTypeId,
        DateOnly date, AppointmentStatus status, int queueNumber,
        decimal price, DateTimeOffset createdAt, Guid createdBy)
    {
        var appt = new Appointment
        {
            ClinicId     = clinicId,
            BranchId     = branchId,
            PatientId    = patientId,
            DoctorInfoId = doctorInfoId,
            VisitTypeId  = visitTypeId,
            Date         = date,
            Type         = AppointmentType.Queue,
            Status       = status,
            QueueNumber  = queueNumber,
            CreatedAt    = createdAt,
            UpdatedAt    = createdAt,
            CreatedBy    = createdBy,
            UpdatedBy    = createdBy,
        };
        appt.ApplyPrice(price);
        return appt;
    }
}
