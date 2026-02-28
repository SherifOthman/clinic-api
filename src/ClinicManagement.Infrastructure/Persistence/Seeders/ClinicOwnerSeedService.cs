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

            var basicPlan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(p => p.Name == "Basic");

            if (basicPlan == null)
            {
                _logger.LogError("Basic Plan not found");
                return;
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

            _logger.LogInformation("Created demo clinic for owner@clinic.com");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding clinic owner data");
            throw;
        }
    }
}
