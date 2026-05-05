using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Seeds 60 appointments across the past 30 days + today.
/// Mix of statuses: Completed, Cancelled, NoShow, Pending, Waiting, InProgress.
/// Enough to trigger pagination (default 10/page = 6 pages).
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

        if (patients.Count == 0) { _logger.LogWarning("No patients found — skipping appointments"); return; }

        var now   = DateTimeOffset.UtcNow;
        var today = DateOnly.FromDateTime(now.Date);
        var appointments = new List<Appointment>();

        // ── Past appointments (last 30 days) — 50 completed/cancelled/noshow ──
        var pastStatuses = new[]
        {
            AppointmentStatus.Completed, AppointmentStatus.Completed, AppointmentStatus.Completed,
            AppointmentStatus.Completed, AppointmentStatus.Completed, AppointmentStatus.Completed,
            AppointmentStatus.Cancelled, AppointmentStatus.NoShow,
        };

        var times = new[] { new TimeOnly(9,0), new TimeOnly(9,30), new TimeOnly(10,0), new TimeOnly(10,30),
                            new TimeOnly(11,0), new TimeOnly(11,30), new TimeOnly(14,0), new TimeOnly(14,30),
                            new TimeOnly(15,0), new TimeOnly(15,30) };

        for (int day = 1; day <= 30; day++)
        {
            var date = today.AddDays(-day);
            // 1–3 appointments per past day
            var count = day % 3 == 0 ? 3 : day % 2 == 0 ? 2 : 1;

            for (int i = 0; i < count; i++)
            {
                var patientId = patients[(day * 3 + i) % patients.Count];
                var status    = pastStatuses[(day + i) % pastStatuses.Length];
                var time      = times[(day + i) % times.Length];
                var vtId      = i % 2 == 0 ? ctx.VisitTypeId : ctx.VisitType2Id;
                var price     = vtId == ctx.VisitTypeId ? 150m : 80m;

                var appt = new Appointment
                {
                    ClinicId      = ctx.ClinicId,
                    BranchId      = ctx.BranchId,
                    PatientId     = patientId,
                    DoctorInfoId  = ctx.DoctorInfoId,
                    VisitTypeId   = vtId,
                    Date          = date,
                    Type          = AppointmentType.Time,
                    ScheduledTime = time,
                    EndTime       = time.AddMinutes(20),
                    Status        = status,
                    CreatedAt     = now.AddDays(-day),
                    UpdatedAt     = now.AddDays(-day),
                    CreatedBy     = ctx.OwnerUserId,
                    UpdatedBy     = ctx.OwnerUserId,
                };
                appt.ApplyPrice(price);
                appointments.Add(appt);
            }
        }

        // ── Today's appointments — mix of all active statuses ──
        var todayData = new[]
        {
            (new TimeOnly(9,  0), AppointmentStatus.Completed,  ctx.VisitTypeId,  150m),
            (new TimeOnly(9, 30), AppointmentStatus.Completed,  ctx.VisitType2Id,  80m),
            (new TimeOnly(10, 0), AppointmentStatus.Completed,  ctx.VisitTypeId,  150m),
            (new TimeOnly(10,30), AppointmentStatus.InProgress, ctx.VisitTypeId,  150m),
            (new TimeOnly(11, 0), AppointmentStatus.Waiting,    ctx.VisitType2Id,  80m),
            (new TimeOnly(11,30), AppointmentStatus.Waiting,    ctx.VisitTypeId,  150m),
            (new TimeOnly(14, 0), AppointmentStatus.Pending,    ctx.VisitType2Id,  80m),
            (new TimeOnly(14,30), AppointmentStatus.Pending,    ctx.VisitTypeId,  150m),
            (new TimeOnly(15, 0), AppointmentStatus.Pending,    ctx.VisitType2Id,  80m),
            (new TimeOnly(15,30), AppointmentStatus.Pending,    ctx.VisitTypeId,  150m),
        };

        for (int i = 0; i < todayData.Length; i++)
        {
            var (time, status, vtId, price) = todayData[i];
            var patientId = patients[i % patients.Count];

            var appt = new Appointment
            {
                ClinicId      = ctx.ClinicId,
                BranchId      = ctx.BranchId,
                PatientId     = patientId,
                DoctorInfoId  = ctx.DoctorInfoId,
                VisitTypeId   = vtId,
                Date          = today,
                Type          = AppointmentType.Time,
                ScheduledTime = time,
                EndTime       = time.AddMinutes(20),
                Status        = status,
                CreatedAt     = now.AddHours(-2),
                UpdatedAt     = now.AddHours(-1),
                CreatedBy     = ctx.OwnerUserId,
                UpdatedBy     = ctx.OwnerUserId,
            };
            appt.ApplyPrice(price);
            appointments.Add(appt);
        }

        _db.Set<Appointment>().AddRange(appointments);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} demo appointments", appointments.Count);
    }
}
