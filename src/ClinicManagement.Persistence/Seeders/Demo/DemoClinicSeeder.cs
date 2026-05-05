using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Ensures the demo clinic has a visit type and DoctorBranchSchedule so appointments can be seeded.
/// The clinic, branch, members, and subscription are already created by SystemUserSeedService.
/// </summary>
public class DemoClinicSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DemoClinicSeeder> _logger;

    public DemoClinicSeeder(ApplicationDbContext db, ILogger<DemoClinicSeeder> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task<DemoClinicContext?> SeedAsync()
    {
        // Resolve clinic owned by the demo owner
        var owner = await _db.Users.FirstOrDefaultAsync(u => u.Email == "owner@clinic.com");
        if (owner is null) { _logger.LogWarning("Demo owner not found"); return null; }

        var clinic = await _db.Set<Clinic>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.OwnerUserId == owner.Id);
        if (clinic is null) { _logger.LogWarning("Demo clinic not found"); return null; }

        var branch = await _db.Set<ClinicBranch>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.ClinicId == clinic.Id);
        if (branch is null) { _logger.LogWarning("Demo branch not found"); return null; }

        // Resolve doctor member
        var doctorUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == "doctor@clinic.com");
        if (doctorUser is null) { _logger.LogWarning("Demo doctor not found"); return null; }

        var doctorMember = await _db.Set<ClinicMember>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.UserId == doctorUser.Id && m.ClinicId == clinic.Id);
        if (doctorMember is null) { _logger.LogWarning("Demo doctor member not found"); return null; }

        var doctorInfo = await _db.Set<DoctorInfo>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.ClinicMemberId == doctorMember.Id);
        if (doctorInfo is null) { _logger.LogWarning("Demo doctor info not found"); return null; }

        // Ensure DoctorBranchSchedule exists
        var schedule = await _db.Set<DoctorBranchSchedule>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.DoctorInfoId == doctorInfo.Id && s.BranchId == branch.Id);

        if (schedule is null)
        {
            schedule = new DoctorBranchSchedule
            {
                DoctorInfoId  = doctorInfo.Id,
                BranchId      = branch.Id,
                AppointmentType = AppointmentType.Time,
            };
            _db.Set<DoctorBranchSchedule>().Add(schedule);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Demo DoctorBranchSchedule created");
        }

        // Ensure working days (Sun–Thu)
        var hasWorkingDays = await _db.Set<WorkingDay>().IgnoreQueryFilters()
            .AnyAsync(w => w.DoctorBranchScheduleId == schedule.Id);

        if (!hasWorkingDays)
        {
            var workDays = new[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday };
            foreach (var day in workDays)
            {
                _db.Set<WorkingDay>().Add(new WorkingDay
                {
                    DoctorBranchScheduleId = schedule.Id,
                    Day                    = day,
                    StartTime              = new TimeOnly(9, 0),
                    EndTime                = new TimeOnly(17, 0),
                });
            }
            await _db.SaveChangesAsync();
        }

        // Ensure visit types
        var visitTypes = await _db.Set<VisitType>().IgnoreQueryFilters()
            .Where(v => v.DoctorBranchScheduleId == schedule.Id)
            .ToListAsync();

        VisitType vt1, vt2;

        if (visitTypes.Count == 0)
        {
            vt1 = new VisitType { DoctorBranchScheduleId = schedule.Id, Name = "Consultation",  Price = 150m, IsActive = true };
            vt2 = new VisitType { DoctorBranchScheduleId = schedule.Id, Name = "Follow-up",     Price = 80m,  IsActive = true };
            _db.Set<VisitType>().AddRange(vt1, vt2);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Demo visit types created");
        }
        else
        {
            vt1 = visitTypes[0];
            vt2 = visitTypes.Count > 1 ? visitTypes[1] : visitTypes[0];
        }

        // Update doctor appointment type to TimeBased for richer demo
        if (doctorInfo.AppointmentType != AppointmentType.Time)
        {
            doctorInfo.AppointmentType = AppointmentType.Time;
            await _db.SaveChangesAsync();
        }

        return new DemoClinicContext
        {
            ClinicId     = clinic.Id,
            BranchId     = branch.Id,
            OwnerUserId  = owner.Id,
            DoctorUserId = doctorUser.Id,
            DoctorInfoId = doctorInfo.Id,
            VisitTypeId  = vt1.Id,
            VisitType2Id = vt2.Id,
        };
    }
}
