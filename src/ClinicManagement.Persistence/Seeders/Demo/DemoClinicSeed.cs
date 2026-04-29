using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Seeds the demo clinic, branches, users (owner, doctor, receptionist),
/// and all clinic-level configuration needed to test the full app.
/// </summary>
public class DemoClinicSeed
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DemoClinicSeed> _logger;
    private readonly SeedOptions _opts;

    public DemoClinicSeed(ApplicationDbContext db, UserManager<User> userManager,
        ILogger<DemoClinicSeed> logger, SeedOptions opts)
    {
        _db          = db;
        _userManager = userManager;
        _logger      = logger;
        _opts        = opts;
    }

    /// <summary>Returns the seeded clinic, or null if already seeded.</summary>
    public async Task<(Clinic clinic, ClinicBranch mainBranch, ClinicBranch westBranch,
        DoctorInfo ownerDoctor, DoctorInfo staffDoctor, User ownerUser)?> SeedAsync()
    {
        // ── SuperAdmin ────────────────────────────────────────────────────────
        await SeedSuperAdminAsync();

        // ── Clinic owner ──────────────────────────────────────────────────────
        var ownerUser = await EnsureUserAsync(
            _opts.ClinicOwner.Email, "owner", "Dr. John Smith", "+201001234567",
            Gender.Male, _opts.ClinicOwner.Password,
            UserRoles.ClinicOwner, UserRoles.Doctor);
        if (ownerUser is null) return null;

        // Skip if clinic already exists
        var existing = await _db.Set<Clinic>()
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .Include(c => c.Branches)
            .FirstOrDefaultAsync(c => c.OwnerUserId == ownerUser.Id);
        if (existing is not null)
        {
            _logger.LogInformation("Demo clinic already seeded — skipping");
            return null;
        }

        var basicPlan = await _db.Set<SubscriptionPlan>().FirstOrDefaultAsync(p => p.Name == "Basic");
        if (basicPlan is null) { _logger.LogError("Basic plan not found"); return null; }

        var generalPractice = await _db.Set<Specialization>().FirstOrDefaultAsync(s => s.NameEn == "General Practice");
        var cardiology      = await _db.Set<Specialization>().FirstOrDefaultAsync(s => s.NameEn == "Cardiology");

        // ── Clinic ────────────────────────────────────────────────────────────
        var clinic = new Clinic
        {
            Name                  = "Demo Medical Clinic",
            OwnerUserId           = ownerUser.Id,
            SubscriptionPlanId    = basicPlan.Id,
            OnboardingCompleted   = true,
            IsActive              = true,
            CountryCode           = "EG",
            SubscriptionStartDate = DateTimeOffset.UtcNow,
            SubscriptionEndDate   = DateTimeOffset.UtcNow.AddMonths(6),
            TrialEndDate          = DateTimeOffset.UtcNow.AddDays(14),
        };
        _db.Set<Clinic>().Add(clinic);

        // ── Branches ──────────────────────────────────────────────────────────
        var mainBranch = new ClinicBranch
        {
            ClinicId       = clinic.Id,
            Name           = "Main Branch",
            AddressLine    = "123 Medical Street, Downtown Cairo",
            StateGeonameId = 360630,
            CityGeonameId  = 360630,
            IsMainBranch   = true,
            IsActive       = true,
        };
        var westBranch = new ClinicBranch
        {
            ClinicId       = clinic.Id,
            Name           = "West Branch",
            AddressLine    = "456 Health Avenue, Giza",
            StateGeonameId = 360630,
            CityGeonameId  = 360630,
            IsMainBranch   = false,
            IsActive       = true,
        };
        _db.Set<ClinicBranch>().AddRange(mainBranch, westBranch);

        // Branch phone numbers
        _db.Set<ClinicBranchPhoneNumber>().AddRange(
            new ClinicBranchPhoneNumber { ClinicBranchId = mainBranch.Id, PhoneNumber = "+20223456789", Label = "Reception" },
            new ClinicBranchPhoneNumber { ClinicBranchId = mainBranch.Id, PhoneNumber = "+20223456790", Label = "Emergency" },
            new ClinicBranchPhoneNumber { ClinicBranchId = westBranch.Id, PhoneNumber = "+20235678901", Label = "Reception" }
        );

        // ── Subscription ──────────────────────────────────────────────────────
        _db.Set<ClinicSubscription>().Add(new ClinicSubscription
        {
            ClinicId           = clinic.Id,
            SubscriptionPlanId = basicPlan.Id,
            Status             = SubscriptionStatus.Active,
            StartDate          = DateTimeOffset.UtcNow.AddMonths(-1),
            EndDate            = DateTimeOffset.UtcNow.AddMonths(5),
            TrialEndDate       = DateTimeOffset.UtcNow.AddDays(14),
            AutoRenew          = true,
        });

        // ── Owner as clinic member + doctor ───────────────────────────────────
        var ownerMember = new ClinicMember
        {
            PersonId = ownerUser.PersonId,
            UserId   = ownerUser.Id,
            ClinicId = clinic.Id,
            Role     = ClinicMemberRole.Owner,
            IsActive = true,
        };
        _db.Set<ClinicMember>().Add(ownerMember);

        var ownerDoctor = new DoctorInfo
        {
            ClinicMemberId             = ownerMember.Id,
            SpecializationId           = generalPractice?.Id,
            AppointmentType            = AppointmentType.Queue,
            DefaultVisitDurationMinutes = 20,
        };
        _db.Set<DoctorInfo>().Add(ownerDoctor);

        AddPermissions(ownerMember.Id, ClinicMemberRole.Owner);

        // ── Staff doctor ──────────────────────────────────────────────────────
        var doctorUser = await EnsureUserAsync(
            _opts.Doctor.Email, "doctor", "Dr. Sarah Johnson", "+201112345678",
            Gender.Female, _opts.Doctor.Password, UserRoles.Doctor);
        if (doctorUser is null) return null;

        var doctorMember = new ClinicMember
        {
            PersonId = doctorUser.PersonId,
            UserId   = doctorUser.Id,
            ClinicId = clinic.Id,
            Role     = ClinicMemberRole.Doctor,
            IsActive = true,
        };
        _db.Set<ClinicMember>().Add(doctorMember);

        var staffDoctor = new DoctorInfo
        {
            ClinicMemberId             = doctorMember.Id,
            SpecializationId           = cardiology?.Id,
            AppointmentType            = AppointmentType.Time,
            DefaultVisitDurationMinutes = 30,
        };
        _db.Set<DoctorInfo>().Add(staffDoctor);
        AddPermissions(doctorMember.Id, ClinicMemberRole.Doctor);

        // ── Receptionist ──────────────────────────────────────────────────────
        var receptionistUser = await EnsureUserAsync(
            _opts.Receptionist.Email, "receptionist", "Emily Davis", "+201223456789",
            Gender.Female, _opts.Receptionist.Password, UserRoles.Receptionist);
        if (receptionistUser is not null)
        {
            var receptionistMember = new ClinicMember
            {
                PersonId = receptionistUser.PersonId,
                UserId   = receptionistUser.Id,
                ClinicId = clinic.Id,
                Role     = ClinicMemberRole.Receptionist,
                IsActive = true,
            };
            _db.Set<ClinicMember>().Add(receptionistMember);
            AddPermissions(receptionistMember.Id, ClinicMemberRole.Receptionist);
        }

        // ── PatientCounter ────────────────────────────────────────────────────
        _db.Set<PatientCounter>().Add(new PatientCounter { ClinicId = clinic.Id, LastValue = 0 });

        await _db.SaveChangesAsync();
        _logger.LogInformation("Demo clinic seeded: {Name} (Id: {Id})", clinic.Name, clinic.Id);

        return (clinic, mainBranch, westBranch, ownerDoctor, staffDoctor, ownerUser);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task SeedSuperAdminAsync()
    {
        if (await _userManager.FindByEmailAsync(_opts.SuperAdmin.Email) is not null) return;

        var person = new Person { FullName = "System Administrator", Gender = Gender.Male };
        var user = new User
        {
            UserName = "superadmin", Email = _opts.SuperAdmin.Email,
            PhoneNumber = "+966500000000", EmailConfirmed = true,
            PersonId = person.Id, Person = person,
        };
        var result = await _userManager.CreateAsync(user, _opts.SuperAdmin.Password);
        if (!result.Succeeded) { LogError("SuperAdmin", result); return; }
        await _userManager.AddToRoleAsync(user, UserRoles.SuperAdmin);
        _logger.LogInformation("SuperAdmin seeded: {Email}", _opts.SuperAdmin.Email);
    }

    private async Task<User?> EnsureUserAsync(string email, string username, string fullName,
        string phone, Gender gender, string password, params string[] roles)
    {
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null) return existing;

        var person = new Person { FullName = fullName, Gender = gender };
        var user = new User
        {
            UserName = username, Email = email,
            PhoneNumber = phone, EmailConfirmed = true,
            PersonId = person.Id, Person = person,
        };
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded) { LogError(username, result); return null; }
        foreach (var role in roles)
            await _userManager.AddToRoleAsync(user, role);
        _logger.LogInformation("User seeded: {Email}", email);
        return user;
    }

    private void AddPermissions(Guid memberId, ClinicMemberRole role)
    {
        var perms = DefaultPermissions.ForRole(role)
            .Select(p => new ClinicMemberPermission { ClinicMemberId = memberId, Permission = p });
        _db.Set<ClinicMemberPermission>().AddRange(perms);
    }

    private void LogError(string name, IdentityResult result) =>
        _logger.LogError("Failed to create {Name}: {Errors}", name,
            string.Join(", ", result.Errors.Select(e => e.Description)));
}
