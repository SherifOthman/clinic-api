using ClinicManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Seeds working days, visit types, and schedules for all 4 demo doctors.
/// Doctor 1 (owner):   Queue-based, Sun–Thu 09:00–17:00
/// Doctor 2 (staff):   Time-based,  Mon–Fri 08:00–16:00
/// Doctor 3 (Omar):    Queue-based, Sun–Thu 10:00–18:00
/// Doctor 4 (Layla):   Time-based,  Tue–Sat 09:00–15:00
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
        DoctorBranchSchedule doctor3Schedule,
        DoctorBranchSchedule doctor4Schedule,
        VisitType ownerVt1, VisitType ownerVt2, VisitType ownerVt3,
        VisitType staffVt1, VisitType staffVt2, VisitType staffVt3,
        VisitType doc3Vt1,  VisitType doc3Vt2,
        VisitType doc4Vt1,  VisitType doc4Vt2
    )> SeedAsync(DoctorInfo ownerDoctor, DoctorInfo staffDoctor,
                 DoctorInfo doctor3,     DoctorInfo doctor4,
                 ClinicBranch branch)
    {
        // ── Doctor 1: Queue, Sun–Thu ──────────────────────────────────────────
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

        // ── Doctor 2: Time, Mon–Fri ───────────────────────────────────────────
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

        // ── Doctor 3: Queue, Sun–Thu ──────────────────────────────────────────
        var doctor3Schedule = new DoctorBranchSchedule
        {
            DoctorInfoId = doctor3.Id,
            BranchId     = branch.Id,
            IsActive     = true,
        };
        _db.Set<DoctorBranchSchedule>().Add(doctor3Schedule);
        foreach (var day in new[] { 0, 1, 2, 3, 4 }) // Sun–Thu
            _db.Set<WorkingDay>().Add(new WorkingDay
            {
                DoctorBranchScheduleId = doctor3Schedule.Id,
                Day         = (DayOfWeek)day,
                StartTime   = new TimeOnly(10, 0),
                EndTime     = new TimeOnly(18, 0),
                IsAvailable = true,
            });

        // ── Doctor 4: Time, Tue–Sat ───────────────────────────────────────────
        var doctor4Schedule = new DoctorBranchSchedule
        {
            DoctorInfoId = doctor4.Id,
            BranchId     = branch.Id,
            IsActive     = true,
        };
        _db.Set<DoctorBranchSchedule>().Add(doctor4Schedule);
        foreach (var day in new[] { 2, 3, 4, 5, 6 }) // Tue–Sat
            _db.Set<WorkingDay>().Add(new WorkingDay
            {
                DoctorBranchScheduleId = doctor4Schedule.Id,
                Day         = (DayOfWeek)day,
                StartTime   = new TimeOnly(9, 0),
                EndTime     = new TimeOnly(15, 0),
                IsAvailable = true,
            });

        // ── Visit types ───────────────────────────────────────────────────────
        var ownerVt1 = new VisitType { DoctorBranchScheduleId = ownerSchedule.Id,   Name = "Consultation", Price = 150, IsActive = true };
        var ownerVt2 = new VisitType { DoctorBranchScheduleId = ownerSchedule.Id,   Name = "Follow-up",    Price = 80,  IsActive = true };
        var ownerVt3 = new VisitType { DoctorBranchScheduleId = ownerSchedule.Id,   Name = "Emergency",    Price = 250, IsActive = true };

        var staffVt1 = new VisitType { DoctorBranchScheduleId = staffSchedule.Id,   Name = "Consultation", Price = 200, IsActive = true };
        var staffVt2 = new VisitType { DoctorBranchScheduleId = staffSchedule.Id,   Name = "ECG",          Price = 120, IsActive = true };
        var staffVt3 = new VisitType { DoctorBranchScheduleId = staffSchedule.Id,   Name = "Echo",         Price = 300, IsActive = true };

        var doc3Vt1  = new VisitType { DoctorBranchScheduleId = doctor3Schedule.Id, Name = "Consultation", Price = 100, IsActive = true };
        var doc3Vt2  = new VisitType { DoctorBranchScheduleId = doctor3Schedule.Id, Name = "Vaccination",  Price = 60,  IsActive = true };

        var doc4Vt1  = new VisitType { DoctorBranchScheduleId = doctor4Schedule.Id, Name = "Consultation", Price = 180, IsActive = true };
        var doc4Vt2  = new VisitType { DoctorBranchScheduleId = doctor4Schedule.Id, Name = "Procedure",    Price = 350, IsActive = true };

        _db.Set<VisitType>().AddRange(
            ownerVt1, ownerVt2, ownerVt3,
            staffVt1, staffVt2, staffVt3,
            doc3Vt1,  doc3Vt2,
            doc4Vt1,  doc4Vt2);

        await _db.SaveChangesAsync();
        _logger.LogInformation("Demo schedules and visit types seeded (4 doctors)");

        return (ownerSchedule, staffSchedule, doctor3Schedule, doctor4Schedule,
                ownerVt1, ownerVt2, ownerVt3,
                staffVt1, staffVt2, staffVt3,
                doc3Vt1,  doc3Vt2,
                doc4Vt1,  doc4Vt2);
    }
}
