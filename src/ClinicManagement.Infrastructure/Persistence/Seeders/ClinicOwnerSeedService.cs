using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds the demo ClinicOwner user and their clinic data.
/// Credentials are configured via SeedOptions (appsettings / user secrets).
/// </summary>
public class ClinicOwnerSeedService
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ClinicOwnerSeedService> _logger;
    private readonly SeedOptions _options;

    public ClinicOwnerSeedService(
        IApplicationDbContext context,
        UserManager<User> userManager,
        ILogger<ClinicOwnerSeedService> logger,
        IOptions<SeedOptions> options)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _options = options.Value;
    }

    public async Task SeedAsync()
    {
        var email = _options.ClinicOwner.Email;

        var owner = await _userManager.FindByEmailAsync(email);
        if (owner == null)
        {
            owner = new User
            {
                UserName = "owner",
                Email = email,
                FirstName = "John",
                LastName = "Smith",
                PhoneNumber = "+1234567890",
                EmailConfirmed = true,
                IsMale = true,
            };

            var result = await _userManager.CreateAsync(owner, _options.ClinicOwner.Password);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to create ClinicOwner: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }

            await _userManager.AddToRoleAsync(owner, "ClinicOwner");
            await _userManager.AddToRoleAsync(owner, "Doctor");
            _logger.LogInformation("ClinicOwner user seeded: {Email}", email);
        }

        // Skip if clinic already exists
        var existingClinic = await _context.Clinics
            .IgnoreQueryFilters([Domain.Common.Constants.QueryFilterNames.Tenant])
            .FirstOrDefaultAsync(c => c.OwnerUserId == owner.Id);

        if (existingClinic != null)
        {
            _logger.LogInformation("Demo clinic already exists, skipping");
            return;
        }

        var basicPlan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Name == "Basic");
        if (basicPlan == null)
        {
            _logger.LogError("Basic subscription plan not found — run SubscriptionPlanSeedService first");
            return;
        }

        var generalPractice = await _context.Specializations.FirstOrDefaultAsync(s => s.NameEn == "General Practice");

        // Clinic
        var clinic = new Clinic
        {
            Name = "Demo Medical Clinic",
            OwnerUserId = owner.Id,
            SubscriptionPlanId = basicPlan.Id,
            OnboardingCompleted = true,
            IsActive = true,
            SubscriptionStartDate = DateTime.UtcNow,
            SubscriptionEndDate = DateTime.UtcNow.AddMonths(1),
            TrialEndDate = DateTime.UtcNow.AddDays(14),
        };
        _context.Clinics.Add(clinic);

        // Branch
        var branch = new ClinicBranch
        {
            ClinicId = clinic.Id,
            Name = "Main Branch",
            AddressLine = "123 Medical Street, Downtown",
            CountryGeoNameId = 6252001, // United States
            StateGeoNameId = 5332921,   // California
            CityGeoNameId = 5368361,    // Los Angeles
            IsMainBranch = true,
            IsActive = true,
        };
        _context.ClinicBranches.Add(branch);

        // Subscription
        var subscription = new ClinicSubscription
        {
            ClinicId = clinic.Id,
            SubscriptionPlanId = basicPlan.Id,
            Status = SubscriptionStatus.Trial,
            StartDate = DateTime.UtcNow,
            TrialEndDate = DateTime.UtcNow.AddDays(14),
            AutoRenew = true,
        };
        _context.ClinicSubscriptions.Add(subscription);

        // Staff record for owner (also a Doctor)
        var staff = new Staff
        {
            UserId = owner.Id,
            ClinicId = clinic.Id,
            IsActive = true,
        };
        _context.Staff.Add(staff);

        var doctorProfile = new DoctorProfile
        {
            StaffId = staff.Id,
            SpecializationId = generalPractice?.Id,
        };
        _context.DoctorProfiles.Add(doctorProfile);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Demo clinic seeded for owner@clinic.com (ClinicId: {ClinicId})", clinic.Id);
    }
}
