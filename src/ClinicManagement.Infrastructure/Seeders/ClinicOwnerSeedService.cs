using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Seeders;

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
            // Check if owner user exists
            var ownerUser = await _unitOfWork.Users.GetByEmailAsync("owner@clinic.com");

            if (ownerUser == null)
            {
                _logger.LogInformation("Clinic Owner user not found, skipping clinic seed");
                return;
            }

            // Check if clinic already exists
            var existingClinic = await _unitOfWork.Clinics.GetByOwnerUserIdAsync(ownerUser.Id);

            if (existingClinic != null)
            {
                _logger.LogInformation("Demo clinic already exists");
                return;
            }

            // Get Basic Plan
            var subscriptionPlans = await _unitOfWork.SubscriptionPlans.GetAllAsync();
            var basicPlan = subscriptionPlans.FirstOrDefault(p => p.Name == "Basic");

            if (basicPlan == null)
            {
                _logger.LogError("Basic Plan not found");
                return;
            }

            await _unitOfWork.BeginTransactionAsync();

            // Create clinic
            var clinic = new Clinic
            {
                Name = "Demo Medical Clinic",
                OwnerUserId = ownerUser.Id,
                SubscriptionPlanId = basicPlan.Id,
                OnboardingCompleted = true,
                IsActive = true
            };

            await _unitOfWork.Clinics.AddAsync(clinic);

            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Created demo clinic for owner@clinic.com");

            // Note: Clinic owner does NOT get a Staff record by default
            // If the owner also works at the clinic (e.g., as a doctor), they should be added as staff separately
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error seeding clinic owner data");
            throw;
        }
    }
}
