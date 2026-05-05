using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Sets up the demo clinic with 3 doctors:
///   Doctor 1 (doctor@clinic.com)  — Time-based, General Practice
///   Doctor 2 (doctor2@clinic.com) — Queue-based, Pediatrics
///   Doctor 3 (doctor3@clinic.com) — Time-based, Cardiology
///
/// Each doctor gets their own DoctorBranchSchedule, working days, and visit types.
/// </summary>
public class DemoClinicSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DemoClinicSeeder> _logger;

    public DemoClinicSeeder(
        ApplicationDbContext db,
        UserManager<User> userManager,
        ILogger<DemoClinicSeeder> logger)
    {
        _db          = db;
        _userManager = userManager;
        _logger      = logger;
    }

    public async Task<DemoClinicContext?> SeedAsync()
    {
        var owner = await _db.Users.FirstOrDefaultAsync(u => u.Email == "owner@clinic.com");
        if (owner is null) { _logger.LogWarning("Demo owner not found"); return null; }

        var clinic = await _db.Set<Clinic>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.OwnerUserId == owner.Id);
        if (clinic is null) { _logger.LogWarning("Demo clinic not found"); return null; }

        var branch = await _db.Set<ClinicBranch>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(b => b.ClinicId == clinic.Id);
        if (branch is null) { _logger.LogWarning("Demo branch not found"); return null; }

        // ── Specializations ───────────────────────────────────────────────────
        var specGeneral    = await _db.Set<Specialization>().FirstOrDefaultAsync(s => s.NameEn == "General Practice");
        var specPediatrics = await _db.Set<Specialization>().FirstOrDefaultAsync(s => s.NameEn == "Pediatrics");
        var specCardiology = await _db.Set<Specialization>().FirstOrDefaultAsync(s => s.NameEn == "Cardiology");

        // ── Doctor 1: Time-based (existing demo doctor) ───────────────────────
        var doc1 = await SetupDoctorAsync(
            email: "doctor@clinic.com", username: "doctor",
            fullName: "Dr. Demo Doctor", phone: "+201112345678",
            gender: Gender.Female, password: "Doctor123!",
            clinicId: clinic.Id, branchId: branch.Id,
            specializationId: specGeneral?.Id,
            appointmentType: AppointmentType.Time,
            scheduleType: AppointmentType.Time,
            workStart: new TimeOnly(9, 0), workEnd: new TimeOnly(17, 0),
            visitTypes: [("Consultation", 150m), ("Follow-up", 80m)]);

        if (doc1 is null) return null;

        // ── Doctor 2: Queue-based (new) ───────────────────────────────────────
        var doc2 = await SetupDoctorAsync(
            email: "doctor2@clinic.com", username: "doctor2",
            fullName: "Dr. Fatima Al-Zahra", phone: "+201112345679",
            gender: Gender.Female, password: "Doctor123!",
            clinicId: clinic.Id, branchId: branch.Id,
            specializationId: specPediatrics?.Id,
            appointmentType: AppointmentType.Queue,
            scheduleType: AppointmentType.Queue,
            workStart: new TimeOnly(8, 0), workEnd: new TimeOnly(14, 0),
            visitTypes: [("General Checkup", 100m), ("Vaccination", 50m)]);

        if (doc2 is null) return null;

        // ── Doctor 3: Time-based, different hours (new) ───────────────────────
        var doc3 = await SetupDoctorAsync(
            email: "doctor3@clinic.com", username: "doctor3",
            fullName: "Dr. Khalid Al-Rashid", phone: "+201112345680",
            gender: Gender.Male, password: "Doctor123!",
            clinicId: clinic.Id, branchId: branch.Id,
            specializationId: specCardiology?.Id,
            appointmentType: AppointmentType.Time,
            scheduleType: AppointmentType.Time,
            workStart: new TimeOnly(14, 0), workEnd: new TimeOnly(20, 0),
            visitTypes: [("Cardiology Consult", 200m), ("ECG", 120m)]);

        if (doc3 is null) return null;

        _logger.LogInformation("Demo clinic setup complete — 3 doctors configured");

        return new DemoClinicContext
        {
            ClinicId      = clinic.Id,
            BranchId      = branch.Id,
            OwnerUserId   = owner.Id,
            DoctorUserId  = doc1.UserId,
            DoctorInfoId  = doc1.DoctorInfoId,
            VisitTypeId   = doc1.VisitType1Id,
            VisitType2Id  = doc1.VisitType2Id,
            Doctor2InfoId = doc2.DoctorInfoId,
            VisitType3Id  = doc2.VisitType1Id,
            VisitType4Id  = doc2.VisitType2Id,
            Doctor3InfoId = doc3.DoctorInfoId,
            VisitType5Id  = doc3.VisitType1Id,
            VisitType6Id  = doc3.VisitType2Id,
        };
    }

    // ── Per-doctor setup ──────────────────────────────────────────────────────

    private async Task<DoctorSetupResult?> SetupDoctorAsync(
        string email, string username, string fullName, string phone,
        Gender gender, string password,
        Guid clinicId, Guid branchId,
        Guid? specializationId,
        AppointmentType appointmentType,
        AppointmentType scheduleType,
        TimeOnly workStart, TimeOnly workEnd,
        (string Name, decimal Price)[] visitTypes)
    {
        // Ensure user exists
        var user = await _userManager.FindByEmailAsync(email)
                   ?? await CreateUserAsync(email, username, fullName, phone, gender, password);
        if (user is null) return null;

        // Ensure clinic member
        var member = await _db.Set<ClinicMember>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.UserId == user.Id && m.ClinicId == clinicId);

        if (member is null)
        {
            member = new ClinicMember
            {
                UserId   = user.Id,
                ClinicId = clinicId,
                Role     = ClinicMemberRole.Doctor,
                IsActive = true,
            };
            _db.Set<ClinicMember>().Add(member);
            await _db.SaveChangesAsync();

            // Default permissions
            var permissions = DefaultPermissions.ForRole(ClinicMemberRole.Doctor)
                .Select(p => new ClinicMemberPermission { ClinicMemberId = member.Id, Permission = p });
            _db.Set<ClinicMemberPermission>().AddRange(permissions);
            await _db.SaveChangesAsync();
        }

        // Ensure DoctorInfo
        var doctorInfo = await _db.Set<DoctorInfo>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(d => d.ClinicMemberId == member.Id);

        if (doctorInfo is null)
        {
            doctorInfo = new DoctorInfo
            {
                ClinicMemberId        = member.Id,
                SpecializationId      = specializationId,
                CanSelfManageSchedule = true,
                AppointmentType       = appointmentType,
            };
            _db.Set<DoctorInfo>().Add(doctorInfo);
            await _db.SaveChangesAsync();
        }
        else if (doctorInfo.AppointmentType != appointmentType)
        {
            doctorInfo.AppointmentType = appointmentType;
            await _db.SaveChangesAsync();
        }

        // Ensure DoctorBranchSchedule
        var schedule = await _db.Set<DoctorBranchSchedule>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.DoctorInfoId == doctorInfo.Id && s.BranchId == branchId);

        if (schedule is null)
        {
            schedule = new DoctorBranchSchedule
            {
                DoctorInfoId    = doctorInfo.Id,
                BranchId        = branchId,
                AppointmentType = scheduleType,
            };
            _db.Set<DoctorBranchSchedule>().Add(schedule);
            await _db.SaveChangesAsync();
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
                    Day       = day,
                    StartTime = workStart,
                    EndTime   = workEnd,
                });
            }
            await _db.SaveChangesAsync();
        }

        // Ensure visit types
        var existingVts = await _db.Set<VisitType>().IgnoreQueryFilters()
            .Where(v => v.DoctorBranchScheduleId == schedule.Id)
            .ToListAsync();

        if (existingVts.Count == 0)
        {
            foreach (var (name, price) in visitTypes)
            {
                existingVts.Add(new VisitType
                {
                    DoctorBranchScheduleId = schedule.Id,
                    Name     = name,
                    Price    = price,
                    IsActive = true,
                });
            }
            _db.Set<VisitType>().AddRange(existingVts);
            await _db.SaveChangesAsync();
        }

        return new DoctorSetupResult(
            user.Id,
            doctorInfo.Id,
            existingVts[0].Id,
            existingVts.Count > 1 ? existingVts[1].Id : existingVts[0].Id);
    }

    private async Task<User?> CreateUserAsync(
        string email, string username, string fullName,
        string phone, Gender gender, string password)
    {
        var user = new User
        {
            UserName       = username,
            Email          = email,
            PhoneNumber    = phone,
            EmailConfirmed = true,
            FullName       = fullName,
            Gender         = gender,
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed to create demo doctor {Email}: {Errors}",
                email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return null;
        }

        await _userManager.AddToRoleAsync(user, UserRoles.Doctor);
        _logger.LogInformation("Demo doctor created: {Email}", email);
        return user;
    }

    private sealed record DoctorSetupResult(
        Guid UserId, Guid DoctorInfoId, Guid VisitType1Id, Guid VisitType2Id);
}
