using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Persistence.Seeders;

public class SubscriptionPlanSeedService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<SubscriptionPlanSeedService> _logger;

    public SubscriptionPlanSeedService(
        IApplicationDbContext context,
        ILogger<SubscriptionPlanSeedService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedSubscriptionPlansAsync()
    {
        var hasPlans = await _context.SubscriptionPlans.AnyAsync();
        if (hasPlans)
        {
            _logger.LogInformation("Subscription plans already seeded");
            return;
        }

        var plans = new[]
        {
            new SubscriptionPlan
            {
                Name = "Starter",
                NameAr = "المبتدئ",
                Description = "Perfect for solo practitioners and small clinics just getting started",
                DescriptionAr = "مثالي للممارسين الفرديين والعيادات الصغيرة التي بدأت للتو",
                MonthlyFee = 199.00m,
                YearlyFee = 1990.00m,
                SetupFee = 0m,
                MaxBranches = 1,
                MaxStaff = 3,
                MaxPatientsPerMonth = 200,
                MaxAppointmentsPerMonth = 500,
                MaxInvoicesPerMonth = 200,
                StorageLimitGB = 2,
                HasInventoryManagement = false,
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
            },
            new SubscriptionPlan
            {
                Name = "Basic",
                NameAr = "أساسي",
                Description = "Ideal for growing clinics with essential features and moderate usage",
                DescriptionAr = "مثالي للعيادات المتنامية مع الميزات الأساسية والاستخدام المعتدل",
                MonthlyFee = 399.00m,
                YearlyFee = 3990.00m,
                SetupFee = 0m,
                MaxBranches = 1,
                MaxStaff = 8,
                MaxPatientsPerMonth = 500,
                MaxAppointmentsPerMonth = 1500,
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
                DisplayOrder = 2,
                EffectiveDate = DateTime.UtcNow
            },
            new SubscriptionPlan
            {
                Name = "Professional",
                NameAr = "احترافي",
                Description = "Advanced features for established clinics with multiple staff members",
                DescriptionAr = "ميزات متقدمة للعيادات القائمة مع عدة موظفين",
                MonthlyFee = 699.00m,
                YearlyFee = 6990.00m,
                SetupFee = 0m,
                MaxBranches = 3,
                MaxStaff = 20,
                MaxPatientsPerMonth = 2000,
                MaxAppointmentsPerMonth = 5000,
                MaxInvoicesPerMonth = 2000,
                StorageLimitGB = 20,
                HasInventoryManagement = true,
                HasReporting = true,
                HasAdvancedReporting = true,
                HasApiAccess = true,
                HasMultipleBranches = true,
                HasCustomBranding = true,
                HasPrioritySupport = false,
                HasBackupAndRestore = true,
                HasIntegrations = true,
                IsActive = true,
                IsPopular = true,
                DisplayOrder = 3,
                EffectiveDate = DateTime.UtcNow
            },
            new SubscriptionPlan
            {
                Name = "Enterprise",
                NameAr = "المؤسسات",
                Description = "Complete solution for large clinics and healthcare networks",
                DescriptionAr = "حل كامل للعيادات الكبيرة وشبكات الرعاية الصحية",
                MonthlyFee = 1299.00m,
                YearlyFee = 12990.00m,
                SetupFee = 500m,
                MaxBranches = 10,
                MaxStaff = 100,
                MaxPatientsPerMonth = 10000,
                MaxAppointmentsPerMonth = 25000,
                MaxInvoicesPerMonth = 10000,
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
                DisplayOrder = 4,
                EffectiveDate = DateTime.UtcNow
            }
        };

        _context.SubscriptionPlans.AddRange(plans);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} subscription plans", plans.Length);
    }
}
