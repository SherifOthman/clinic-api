using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Persistence.Seeders;

/// <summary>
/// Seeds all four demo login accounts and their clinic/staff relationships.
/// Controlled by Seed:SeedDemoUsers in appsettings.
/// Set to false in appsettings.Production.json when demo accounts are no longer needed.
/// </summary>
public class DemoUsersSeedService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DemoUsersSeedService> _logger;
    private readonly SeedOptions _options;

    public DemoUsersSeedService(
        ApplicationDbContext context,
        UserManager<User> userManager,
        ILogger<DemoUsersSeedService> logger,
        IOptions<SeedOptions> options)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _options = options.Value;
    }

    public async Task SeedAsync()
    {
        if (!_options.SeedDemoUsers)
        {
            _logger.LogInformation("Demo user seeding is disabled (Seed:SeedDemoUsers = false)");
            return;
        }

        await SeedSuperAdminAsync();
        var clinic = await SeedClinicOwnerAsync();
        if (clinic is null) return;
        await SeedDoctorAsync(clinic);
        await SeedReceptionistAsync(clinic);
    }

    // ── SuperAdmin ────────────────────────────────────────────────────────────

    private async Task SeedSuperAdminAsync()
    {
        var opts = _options.SuperAdmin;
        if (await _userManager.FindByEmailAsync(opts.Email) != null) return;

        var user = new User
        {
            UserName = "superadmin", Email = opts.Email,
            FirstName = "System", LastName = "Administrator",
            PhoneNumber = "+966500000000", EmailConfirmed = true, Gender = Gender.Male,
        };

        var result = await _userManager.CreateAsync(user, opts.Password);
        if (!result.Succeeded) { LogError("SuperAdmin", result); return; }

        await _userManager.AddToRoleAsync(user, UserRoles.SuperAdmin);
        _logger.LogInformation("SuperAdmin seeded: {Email}", opts.Email);
    }

    // ── Clinic Owner ──────────────────────────────────────────────────────────

    private async Task<Clinic?> SeedClinicOwnerAsync()
    {
        var opts = _options.ClinicOwner;

        var owner = await _userManager.FindByEmailAsync(opts.Email);
        if (owner == null)
        {
            owner = new User
            {
                UserName = "owner", Email = opts.Email,
                FirstName = "John", LastName = "Smith",
                PhoneNumber = "+1234567890", EmailConfirmed = true, Gender = Gender.Male,
            };

            var result = await _userManager.CreateAsync(owner, opts.Password);
            if (!result.Succeeded) { LogError("ClinicOwner", result); return null; }

            await _userManager.AddToRoleAsync(owner, UserRoles.ClinicOwner);
            await _userManager.AddToRoleAsync(owner, UserRoles.Doctor);
            _logger.LogInformation("ClinicOwner seeded: {Email}", opts.Email);
        }

        // Return existing clinic if already set up
        var existing = await _context.Set<Clinic>()
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .FirstOrDefaultAsync(c => c.OwnerUserId == owner.Id);
        if (existing != null) return existing;

        var basicPlan = await _context.Set<SubscriptionPlan>().FirstOrDefaultAsync(p => p.Name == "Basic");
        if (basicPlan == null) { _logger.LogError("Basic plan not found — run SubscriptionPlanSeedService first"); return null; }

        var generalPractice = await _context.Set<Specialization>().FirstOrDefaultAsync(s => s.NameEn == "General Practice");

        var clinic = new Clinic
        {
            Name = "Demo Medical Clinic", OwnerUserId = owner.Id,
            SubscriptionPlanId = basicPlan.Id, OnboardingCompleted = true, IsActive = true,
            SubscriptionStartDate = DateTimeOffset.UtcNow,
            SubscriptionEndDate = DateTimeOffset.UtcNow.AddMonths(1),
            TrialEndDate = DateTimeOffset.UtcNow.AddDays(14),
        };
        _context.Set<Clinic>().Add(clinic);

        _context.Set<ClinicBranch>().Add(new ClinicBranch
        {
            ClinicId = clinic.Id, Name = "Main Branch",
            AddressLine = "123 Medical Street, Downtown",
            CityNameEn = "Cairo", CityNameAr = "القاهرة",
            StateNameEn = "Cairo Governorate", StateNameAr = "محافظة القاهرة",
            IsMainBranch = true, IsActive = true,
        });

        _context.Set<ClinicSubscription>().Add(new ClinicSubscription
        {
            ClinicId = clinic.Id, SubscriptionPlanId = basicPlan.Id,
            Status = SubscriptionStatus.Trial,
            StartDate = DateTimeOffset.UtcNow, TrialEndDate = DateTimeOffset.UtcNow.AddDays(14), AutoRenew = true,
        });

        var ownerStaff = new Staff { UserId = owner.Id, ClinicId = clinic.Id, IsActive = true };
        _context.Set<Staff>().Add(ownerStaff);
        _context.Set<DoctorProfile>().Add(new DoctorProfile { StaffId = ownerStaff.Id, SpecializationId = generalPractice?.Id });

        await _context.SaveChangesAsync();
        _logger.LogInformation("Demo clinic seeded (ClinicId: {Id})", clinic.Id);
        return clinic;
    }

    // ── Doctor ────────────────────────────────────────────────────────────────

    private async Task SeedDoctorAsync(Clinic clinic)
    {
        var opts = _options.Doctor;

        var doctor = await _userManager.FindByEmailAsync(opts.Email);
        if (doctor == null)
        {
            doctor = new User
            {
                UserName = "doctor", Email = opts.Email,
                FirstName = "Sarah", LastName = "Johnson",
                PhoneNumber = "+1234567891", EmailConfirmed = true, Gender = Gender.Female,
            };

            var result = await _userManager.CreateAsync(doctor, opts.Password);
            if (!result.Succeeded) { LogError("Doctor", result); return; }

            await _userManager.AddToRoleAsync(doctor, UserRoles.Doctor);
            _logger.LogInformation("Doctor seeded: {Email}", opts.Email);
        }

        var alreadyStaff = await _context.Set<Staff>()
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .AnyAsync(s => s.UserId == doctor.Id && s.ClinicId == clinic.Id);
        if (alreadyStaff) return;

        var cardiology = await _context.Set<Specialization>().FirstOrDefaultAsync(s => s.NameEn == "Cardiology");
        var staff = new Staff { UserId = doctor.Id, ClinicId = clinic.Id, IsActive = true };
        _context.Set<Staff>().Add(staff);
        _context.Set<DoctorProfile>().Add(new DoctorProfile { StaffId = staff.Id, SpecializationId = cardiology?.Id });
        await _context.SaveChangesAsync();
    }

    // ── Receptionist ──────────────────────────────────────────────────────────

    private async Task SeedReceptionistAsync(Clinic clinic)
    {
        var opts = _options.Receptionist;

        var receptionist = await _userManager.FindByEmailAsync(opts.Email);
        if (receptionist == null)
        {
            receptionist = new User
            {
                UserName = "receptionist", Email = opts.Email,
                FirstName = "Emily", LastName = "Davis",
                PhoneNumber = "+1234567892", EmailConfirmed = true, Gender = Gender.Female,
            };

            var result = await _userManager.CreateAsync(receptionist, opts.Password);
            if (!result.Succeeded) { LogError("Receptionist", result); return; }

            await _userManager.AddToRoleAsync(receptionist, UserRoles.Receptionist);
            _logger.LogInformation("Receptionist seeded: {Email}", opts.Email);
        }

        var alreadyStaff = await _context.Set<Staff>()
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .AnyAsync(s => s.UserId == receptionist.Id && s.ClinicId == clinic.Id);
        if (alreadyStaff) return;

        _context.Set<Staff>().Add(new Staff { UserId = receptionist.Id, ClinicId = clinic.Id, IsActive = true });
        await _context.SaveChangesAsync();
    }

    private void LogError(string role, IdentityResult result) =>
        _logger.LogError("Failed to create {Role}: {Errors}", role, string.Join(", ", result.Errors.Select(e => e.Description)));
}
