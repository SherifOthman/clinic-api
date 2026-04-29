using ClinicManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Seeds working days, visit types, and schedules for both demo doctors.
/// Owner doctor: Queue-based, Sun–Thu 09:00–17:00
/// Staff doctor: Time-based, Mon–Fri 08:00–16:00
/// </summary>
public class DemoScheduleSeed
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DemoScheduleSeed> _logger;

    public DemoScheduleSeed(ApplicationDbContext db, ILogger<DemoScheduleSeed> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task<(
        DoctorBranchSchedule ownerSchedule,
        DoctorBranchSchedule staffSchedule,
        VisitType ownerVt1, VisitType ownerVt2, VisitType ownerVt3,
        VisitType staffVt1, VisitType staffVt2, VisitType staffVt3
    )> SeedAsync(DoctorInfo ownerDoctor, DoctorInfo staffDoctor, ClinicBranch branch)
    {
        // ── Owner schedule: Queue, Sun–Thu ────────────────────────────────────
        var ownerSchedule = new DoctorBranchSchedule
        {
            DoctorInfoId = ownerDoctor.Id,
            BranchId     = branch.Id,
            IsActive     = true,
        };
        _db.Set<DoctorBranchSchedule>().Add(ownerSchedule);

        foreach (var day in new[] { 0, 1, 2, 3, 4 }) // Sun–Thu
            _db.Set<WorkingDay>().Add(new WorkingDay
            {
                DoctorBranchScheduleId = ownerSchedule.Id,
                Day         = (DayOfWeek)day,
                StartTime   = new TimeOnly(9, 0),
                EndTime     = new TimeOnly(17, 0),
                IsAvailable = true,
            });

        // ── Staff schedule: Time, Mon–Fri ─────────────────────────────────────
        var staffSchedule = new DoctorBranchSchedule
        {
            DoctorInfoId = staffDoctor.Id,
            BranchId     = branch.Id,
            IsActive     = true,
        };
        _db.Set<DoctorBranchSchedule>().Add(staffSchedule);

        foreach (var day in new[] { 1, 2, 3, 4, 5 }) // Mon–Fri
            _db.Set<WorkingDay>().Add(new WorkingDay
            {
                DoctorBranchScheduleId = staffSchedule.Id,
                Day         = (DayOfWeek)day,
                StartTime   = new TimeOnly(8, 0),
                EndTime     = new TimeOnly(16, 0),
                IsAvailable = true,
            });

        // ── Visit types ───────────────────────────────────────────────────────
        var ownerVt1 = new VisitType { DoctorBranchScheduleId = ownerSchedule.Id, Name = "Consultation", Price = 150, IsActive = true };
        var ownerVt2 = new VisitType { DoctorBranchScheduleId = ownerSchedule.Id, Name = "Follow-up",    Price = 80,  IsActive = true };
        var ownerVt3 = new VisitType { DoctorBranchScheduleId = ownerSchedule.Id, Name = "Emergency",    Price = 250, IsActive = true };
        var staffVt1 = new VisitType { DoctorBranchScheduleId = staffSchedule.Id, Name = "Consultation", Price = 200, IsActive = true };
        var staffVt2 = new VisitType { DoctorBranchScheduleId = staffSchedule.Id, Name = "ECG",          Price = 120, IsActive = true };
        var staffVt3 = new VisitType { DoctorBranchScheduleId = staffSchedule.Id, Name = "Echo",         Price = 300, IsActive = true };

        _db.Set<VisitType>().AddRange(ownerVt1, ownerVt2, ownerVt3, staffVt1, staffVt2, staffVt3);

        await _db.SaveChangesAsync();
        _logger.LogInformation("Demo schedules and visit types seeded");

        return (ownerSchedule, staffSchedule, ownerVt1, ownerVt2, ownerVt3, staffVt1, staffVt2, staffVt3);
    }
}
