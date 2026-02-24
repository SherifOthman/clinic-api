using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Persistence.Seeders;

public class ClinicOwnerSeedService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ClinicOwnerSeedService> _logger;

    public ClinicOwnerSeedService(
        IUnitOfWork unitOfWork,
        ILogger<ClinicOwnerSeedService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task SeedClinicOwnerDataAsync()
    {
        try
        {
            var ownerUser = await _unitOfWork.Users.GetByEmailAsync("owner@clinic.com");

            if (ownerUser == null)
            {
                _logger.LogInformation("Clinic Owner user not found, skipping clinic seed");
                return;
            }

            var existingClinic = await _unitOfWork.Clinics.GetByOwnerUserIdAsync(ownerUser.Id);

            if (existingClinic != null)
            {
                _logger.LogInformation("Demo clinic already exists");
                return;
            }

            var subscriptionPlans = await _unitOfWork.SubscriptionPlans.GetAllAsync();
            var basicPlan = subscriptionPlans.FirstOrDefault(p => p.Name == "Basic");

            if (basicPlan == null)
            {
                _logger.LogError("Basic Plan not found");
                return;
            }

            // Get user roles to determine if owner is also a doctor/receptionist
            var userRoles = await _unitOfWork.Users.GetUserRolesAsync(ownerUser.Id);
            var isDoctorRole = userRoles.Contains(UserRoles.Doctor);
            var isReceptionistRole = userRoles.Contains(UserRoles.Receptionist);

            await _unitOfWork.BeginTransactionAsync();

            // Create clinic with subscription dates
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

            await _unitOfWork.Clinics.AddAsync(clinic);

            // Create ClinicSubscription record with Trial status
            var subscription = new ClinicSubscription
            {
                ClinicId = clinic.Id,
                SubscriptionPlanId = basicPlan.Id,
                Status = SubscriptionStatus.Trial,
                StartDate = DateTime.UtcNow,
                TrialEndDate = DateTime.UtcNow.AddDays(14),
                AutoRenew = true
            };

            await _unitOfWork.ClinicSubscriptions.AddAsync(subscription);

            // If owner has Doctor or Receptionist role, create Staff record
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

                await _unitOfWork.Staff.AddAsync(staff);

                // If owner is a doctor, create DoctorProfile
                if (isDoctorRole)
                {
                    // Get General Practice specialization
                    var specializations = await _unitOfWork.Specializations.GetAllAsync();
                    var generalPractice = specializations.FirstOrDefault(s => s.NameEn == "General Practice");

                    if (generalPractice != null)
                    {
                        var doctorProfile = new DoctorProfile
                        {
                            StaffId = staff.Id
                        };

                        await _unitOfWork.DoctorProfiles.AddAsync(doctorProfile);

                        // Create DoctorSpecialization with primary specialization
                        var doctorSpecialization = new DoctorSpecialization
                        {
                            DoctorProfileId = doctorProfile.Id,
                            SpecializationId = generalPractice.Id,
                            IsPrimary = true,
                            YearsOfExperience = 0
                        };

                        await _unitOfWork.DoctorSpecializations.AddAsync(doctorSpecialization);
                    }
                }
            }

            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Created demo clinic for owner@clinic.com");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error seeding clinic owner data");
            throw;
        }
    }
}
