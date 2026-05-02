using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders;

/// <summary>
/// Seeds the four system users plus a fully configured demo clinic.
/// Everything is hardcoded — no JSON config needed.
///
/// What gets created (idempotent — skips if already exists):
///   Users:   superadmin, owner, doctor, receptionist
///   Clinic:  Demo Clinic (owned by owner)
///   Branch:  Main Branch
///   Members: owner + doctor + receptionist linked to the clinic
///   Subscription: Active trial for the clinic
///   Permissions: role defaults for each member
/// </summary>
public class SystemUserSeedService
{
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly ILogger<SystemUserSeedService> _logger;

    // ── Credentials ───────────────────────────────────────────────────────────
    private const string SuperAdminEmail    = "superadmin@clinic.com";
    private const string SuperAdminPassword = "SuperAdmin123!";

    private const string OwnerEmail    = "owner@clinic.com";
    private const string OwnerPassword = "ClinicOwner123!";

    private const string DoctorEmail    = "doctor@clinic.com";
    private const string DoctorPassword = "Doctor123!";

    private const string ReceptionistEmail    = "receptionist@clinic.com";
    private const string ReceptionistPassword = "Receptionist123!";

    public SystemUserSeedService(
        UserManager<User> userManager,
        ApplicationDbContext db,
        ILogger<SystemUserSeedService> logger)
    {
        _userManager = userManager;
        _db          = db;
        _logger      = logger;
    }

    public async Task SeedAsync()
    {
        // ── 1. Users ──────────────────────────────────────────────────────────
        var superAdmin   = await EnsureUserAsync(SuperAdminEmail,   "superadmin",   "System Administrator", "+966500000000", Gender.Male,   SuperAdminPassword,   [UserRoles.SuperAdmin]);
        var owner        = await EnsureUserAsync(OwnerEmail,        "owner",        "Clinic Owner",         "+201001234567", Gender.Male,   OwnerPassword,        [UserRoles.ClinicOwner]);
        var doctor       = await EnsureUserAsync(DoctorEmail,       "doctor",       "Demo Doctor",          "+201112345678", Gender.Female, DoctorPassword,       [UserRoles.Doctor]);
        var receptionist = await EnsureUserAsync(ReceptionistEmail, "receptionist", "Demo Receptionist",    "+201223456789", Gender.Female, ReceptionistPassword, [UserRoles.Receptionist]);

        if (owner is null || doctor is null || receptionist is null) return;

        // ── 2. Clinic ─────────────────────────────────────────────────────────
        var clinic = await _db.Set<Clinic>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.OwnerUserId == owner.Id);

        if (clinic is null)
        {
            // Pick the first active subscription plan
            var plan = await _db.Set<SubscriptionPlan>()
                .FirstOrDefaultAsync(p => p.IsActive);

            if (plan is null)
            {
                _logger.LogWarning("No subscription plans found — skipping clinic seed");
                return;
            }

            clinic = new Clinic
            {
                Name                = "Demo Clinic",
                OwnerUserId         = owner.Id,
                SubscriptionPlanId  = plan.Id,
                OnboardingCompleted = true,
                IsActive            = true,
                CountryCode         = "EG",
                WeekStartDay        = 6,
            };
            _db.Set<Clinic>().Add(clinic);

            // ── 3. Branch ─────────────────────────────────────────────────────
            var branch = new ClinicBranch
            {
                ClinicId     = clinic.Id,
                Name         = "Main Branch",
                AddressLine  = "123 Demo Street, Cairo",
                IsMainBranch = true,
                IsActive     = true,
            };
            _db.Set<ClinicBranch>().Add(branch);

            // ── 4. Subscription ───────────────────────────────────────────────
            var now = DateTimeOffset.UtcNow;
            _db.Set<ClinicSubscription>().Add(new ClinicSubscription
            {
                ClinicId           = clinic.Id,
                SubscriptionPlanId = plan.Id,
                Status             = SubscriptionStatus.Active,
                StartDate          = now,
                EndDate            = now.AddYears(1),
                TrialEndDate       = now.AddDays(30),
                AutoRenew          = true,
            });

            await _db.SaveChangesAsync();
            _logger.LogInformation("Demo clinic created: {ClinicId}", clinic.Id);
        }

        // ── 5. Members ────────────────────────────────────────────────────────
        await EnsureMemberAsync(owner.Id,        clinic.Id, ClinicMemberRole.Owner);
        await EnsureMemberAsync(doctor.Id,       clinic.Id, ClinicMemberRole.Doctor);
        await EnsureMemberAsync(receptionist.Id, clinic.Id, ClinicMemberRole.Receptionist);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<User?> EnsureUserAsync(
        string email, string username, string fullName,
        string phone, Gender gender, string password, string[] roles)
    {
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null) return existing;

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
            _logger.LogError("Failed to seed user {Email}: {Errors}",
                email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return null;
        }

        foreach (var role in roles)
            await _userManager.AddToRoleAsync(user, role);

        _logger.LogInformation("System user seeded: {Email} [{Roles}]",
            email, string.Join(", ", roles));

        return user;
    }

    private async Task EnsureMemberAsync(Guid userId, Guid clinicId, ClinicMemberRole role)
    {
        var exists = await _db.Set<ClinicMember>()
            .IgnoreQueryFilters()
            .AnyAsync(m => m.UserId == userId && m.ClinicId == clinicId);

        if (exists) return;

        var member = new ClinicMember
        {
            UserId   = userId,
            ClinicId = clinicId,
            Role     = role,
            IsActive = true,
        };
        _db.Set<ClinicMember>().Add(member);
        await _db.SaveChangesAsync();

        // Seed default permissions (skip for Owner — handler bypasses permission checks)
        if (role != ClinicMemberRole.Owner)
        {
            var permissions = DefaultPermissions.ForRole(role)
                .Select(p => new ClinicMemberPermission
                {
                    ClinicMemberId = member.Id,
                    Permission     = p,
                });
            _db.Set<ClinicMemberPermission>().AddRange(permissions);
            await _db.SaveChangesAsync();
        }

        // Doctor gets a DoctorInfo record
        if (role == ClinicMemberRole.Doctor)
        {
            var spec = await _db.Set<Specialization>().FirstOrDefaultAsync();
            _db.Set<DoctorInfo>().Add(new DoctorInfo
            {
                ClinicMemberId        = member.Id,
                SpecializationId      = spec?.Id,
                CanSelfManageSchedule = true,
                AppointmentType       = AppointmentType.Queue,
            });
            await _db.SaveChangesAsync();
        }

        _logger.LogInformation("Member seeded: {UserId} as {Role} in clinic {ClinicId}",
            userId, role, clinicId);
    }
}
