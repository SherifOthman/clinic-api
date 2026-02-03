using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Service responsible for seeding default subscription plans
/// </summary>
public class SubscriptionPlanSeedService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<SubscriptionPlanSeedService> _logger;

    public SubscriptionPlanSeedService(IApplicationDbContext context, ILogger<SubscriptionPlanSeedService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seeds the database with default subscription plans if they don't exist
    /// </summary>
    public async Task SeedDefaultPlansAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if plans already exist
            var existingPlansCount = _context.SubscriptionPlans.Count();
            if (existingPlansCount > 0)
            {
                _logger.LogInformation("Subscription plans already exist. Skipping seed.");
                return;
            }

            _logger.LogInformation("Seeding default subscription plans...");

            var plans = new List<SubscriptionPlan>
            {
                // Free Plan
                new SubscriptionPlan
                {
                    Name = "Free",
                    Description = "Perfect for small clinics just getting started",
                    MonthlyFee = 0,
                    YearlyFee = 0,
                    SetupFee = 0,
                    MaxBranches = 1,
                    MaxStaff = 2,
                    MaxPatientsPerMonth = 50,
                    MaxAppointmentsPerMonth = 100,
                    MaxInvoicesPerMonth = 50,
                    StorageLimitGB = 1,
                    HasInventoryManagement = false,
                    HasReporting = true,
                    HasAdvancedReporting = false,
                    HasApiAccess = false,
                    HasMultipleBranches = false,
                    HasCustomBranding = false,
                    HasPrioritySupport = false,
                    HasBackupAndRestore = false,
                    HasIntegrations = false,
                    IsActive = true,
                    IsPopular = false,
                    DisplayOrder = 1
                },

                // Basic Plan
                new SubscriptionPlan
                {
                    Name = "Basic",
                    Description = "Ideal for growing clinics with essential features",
                    MonthlyFee = 29.99m,
                    YearlyFee = 299.99m, // ~17% discount
                    SetupFee = 0,
                    MaxBranches = 1,
                    MaxStaff = 5,
                    MaxPatientsPerMonth = 200,
                    MaxAppointmentsPerMonth = 500,
                    MaxInvoicesPerMonth = 200,
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
                    IsPopular = true,
                    DisplayOrder = 2
                },

                // Professional Plan
                new SubscriptionPlan
                {
                    Name = "Professional",
                    Description = "Advanced features for established clinics",
                    MonthlyFee = 59.99m,
                    YearlyFee = 599.99m, // ~17% discount
                    SetupFee = 0,
                    MaxBranches = 3,
                    MaxStaff = 15,
                    MaxPatientsPerMonth = 1000,
                    MaxAppointmentsPerMonth = 2000,
                    MaxInvoicesPerMonth = 1000,
                    StorageLimitGB = 20,
                    HasInventoryManagement = true,
                    HasReporting = true,
                    HasAdvancedReporting = true,
                    HasApiAccess = true,
                    HasMultipleBranches = true,
                    HasCustomBranding = true,
                    HasPrioritySupport = true,
                    HasBackupAndRestore = true,
                    HasIntegrations = true,
                    IsActive = true,
                    IsPopular = false,
                    DisplayOrder = 3
                },

                // Enterprise Plan
                new SubscriptionPlan
                {
                    Name = "Enterprise",
                    Description = "Complete solution for large clinic networks",
                    MonthlyFee = 149.99m,
                    YearlyFee = 1499.99m, // ~17% discount
                    SetupFee = 199.99m,
                    MaxBranches = int.MaxValue, // Unlimited
                    MaxStaff = int.MaxValue, // Unlimited
                    MaxPatientsPerMonth = int.MaxValue, // Unlimited
                    MaxAppointmentsPerMonth = int.MaxValue, // Unlimited
                    MaxInvoicesPerMonth = int.MaxValue, // Unlimited
                    StorageLimitGB = 100,
                    HasInventoryManagement = true,
                    HasReporting = true,
                    HasAdvancedReporting = true,
                    HasApiAccess = true,
                    HasMultipleBranches = true,
                    HasCustomBranding = true,
                    HasPrioritySupport = true,
                    HasBackupAndRestore = true,
                    HasIntegrations = true,
                    IsActive = true,
                    IsPopular = false,
                    DisplayOrder = 4
                }
            };

            foreach (var plan in plans)
            {
                await _context.SubscriptionPlans.AddAsync(plan, cancellationToken);
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Successfully seeded {Count} subscription plans", plans.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding subscription plans");
            throw;
        }
    }
}
