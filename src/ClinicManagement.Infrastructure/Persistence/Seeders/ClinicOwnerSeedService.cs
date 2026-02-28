using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Persistence.Seeders;

public class ClinicOwnerSeedService
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ClinicOwnerSeedService> _logger;

    public ClinicOwnerSeedService(
        IApplicationDbContext context,
        UserManager<User> userManager,
        ILogger<ClinicOwnerSeedService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedClinicOwnerDataAsync()
    {
        try
        {
            var ownerUser = await _userManager.FindByEmailAsync("owner@clinic.com");

            if (ownerUser == null)
            {
                _logger.LogInformation("Clinic Owner user not found, skipping clinic seed");
                return;
            }

            var existingClinic = await _context.Clinics
                .FirstOrDefaultAsync(c => c.OwnerUserId == ownerUser.Id);

            if (existingClinic != null)
            {
                _logger.LogInformation("Demo clinic already exists");
                return;
            }

            // Seed subscription plan if it doesn't exist
            var basicPlan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Name == "Basic");

            if (basicPlan == null)
            {
                basicPlan = new SubscriptionPlan
                {
                    Name = "Basic",
                    NameAr = "أساسي",
                    Description = "Basic plan for small clinics",
                    DescriptionAr = "خطة أساسية للعيادات الصغيرة",
                    MonthlyFee = 299.00m,
                    YearlyFee = 2990.00m,
                    SetupFee = 0m,
                    MaxBranches = 1,
                    MaxStaff = 5,
                    MaxPatientsPerMonth = 500,
                    MaxAppointmentsPerMonth = 1000,
                    MaxInvoicesPerMonth = 500,
                    StorageLimitGB = 5,
                    HasInventoryManagement = true,
                    HasReporting = true,
                    HasAdvancedReporting = false,
                    HasApiAccess = false,
                    HasMultipleBranches = false,
                    HasCustomBranding = false,
                    HasPrioritySupport = false,
                    HasBackupAndRestore = true,
                    HasIntegrations = false,
                    IsActive = true,
                    IsPopular = false,
                    DisplayOrder = 1,
                    EffectiveDate = DateTime.UtcNow
                };
                
                _context.SubscriptionPlans.Add(basicPlan);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Created Basic subscription plan");
            }

            var userRoles = await _userManager.GetRolesAsync(ownerUser);
            var isDoctorRole = userRoles.Contains(UserRoles.Doctor);
            var isReceptionistRole = userRoles.Contains(UserRoles.Receptionist);

            var clinic = new Clinic
            {
                Name = "Demo Medical Clinic",
                OwnerUserId = ownerUser.Id,
                SubscriptionPlanId = basicPlan.Id,
                OnboardingCompleted = true,
                IsActive = true,
                SubscriptionStartDate = DateTime.UtcNow,
                SubscriptionEndDate = DateTime.UtcNow.AddMonths(1),
                TrialEndDate = DateTime.UtcNow.AddDays(14)
            };

            _context.Clinics.Add(clinic);

            // Create first branch
            var mainBranch = new ClinicBranch
            {
                ClinicId = clinic.Id,
                Name = "Main Branch",
                AddressLine = "123 Medical Street, Downtown",
                CountryGeoNameId = 6252001, // United States
                StateGeoNameId = 5332921, // California
                CityGeoNameId = 5368361, // Los Angeles
                IsMainBranch = true,
                IsActive = true
            };

            _context.ClinicBranches.Add(mainBranch);

            var subscription = new ClinicSubscription
            {
                ClinicId = clinic.Id,
                SubscriptionPlanId = basicPlan.Id,
                Status = SubscriptionStatus.Trial,
                StartDate = DateTime.UtcNow,
                TrialEndDate = DateTime.UtcNow.AddDays(14),
                AutoRenew = true
            };

            _context.ClinicSubscriptions.Add(subscription);

            if (isDoctorRole || isReceptionistRole)
            {
                var staff = new Staff
                {
                    UserId = ownerUser.Id,
                    ClinicId = clinic.Id,
                    IsActive = true,
                    HireDate = DateTime.UtcNow,
                    IsPrimaryClinic = true,
                    Status = StaffStatus.Active
                };

                _context.Staff.Add(staff);

                if (isDoctorRole)
                {
                    var generalPractice = await _context.Specializations
                        .FirstOrDefaultAsync(s => s.NameEn == "General Practice");

                    if (generalPractice != null)
                    {
                        var doctorProfile = new DoctorProfile
                        {
                            StaffId = staff.Id
                        };

                        _context.DoctorProfiles.Add(doctorProfile);

                        var doctorSpecialization = new DoctorSpecialization
                        {
                            DoctorProfileId = doctorProfile.Id,
                            SpecializationId = generalPractice.Id,
                            IsPrimary = true,
                            YearsOfExperience = 0
                        };

                        _context.DoctorSpecializations.Add(doctorSpecialization);
                    }
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Created demo clinic with main branch for owner@clinic.com");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding clinic owner data");
            throw;
        }
    }
}
