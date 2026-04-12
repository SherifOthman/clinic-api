using System.Text.Json;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders;

public class SubscriptionPlanSeedService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SubscriptionPlanSeedService> _logger;

    public SubscriptionPlanSeedService(ApplicationDbContext context, ILogger<SubscriptionPlanSeedService> logger)
    {
        _context = context;
        _logger  = logger;
    }

    public async Task SeedSubscriptionPlansAsync()
    {
        if (await _context.Set<SubscriptionPlan>().AnyAsync()) return;

        var json  = await File.ReadAllTextAsync(ResolvePath("subscription-plans.json"));
        var items = JsonSerializer.Deserialize<PlanSeedDto[]>(json, JsonOptions)!;

        _context.Set<SubscriptionPlan>().AddRange(items.Select(i => new SubscriptionPlan
        {
            Name = i.Name, NameAr = i.NameAr,
            Description = i.Description, DescriptionAr = i.DescriptionAr,
            MonthlyFee = i.MonthlyFee, YearlyFee = i.YearlyFee, SetupFee = i.SetupFee,
            MaxBranches = i.MaxBranches, MaxStaff = i.MaxStaff,
            MaxPatientsPerMonth = i.MaxPatientsPerMonth,
            MaxAppointmentsPerMonth = i.MaxAppointmentsPerMonth,
            MaxInvoicesPerMonth = i.MaxInvoicesPerMonth,
            StorageLimitGB = i.StorageLimitGB,
            HasInventoryManagement = i.HasInventoryManagement,
            HasReporting = i.HasReporting, HasAdvancedReporting = i.HasAdvancedReporting,
            HasApiAccess = i.HasApiAccess, HasMultipleBranches = i.HasMultipleBranches,
            HasCustomBranding = i.HasCustomBranding, HasPrioritySupport = i.HasPrioritySupport,
            HasBackupAndRestore = i.HasBackupAndRestore, HasIntegrations = i.HasIntegrations,
            IsActive = i.IsActive, IsPopular = i.IsPopular, DisplayOrder = i.DisplayOrder,
            EffectiveDate = DateOnly.FromDateTime(DateTime.Today),
        }));

        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} subscription plans", items.Length);
    }

    private static string ResolvePath(string fileName)
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "SeedData", fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "SeedData", fileName),
        };
        return candidates.FirstOrDefault(File.Exists) ?? candidates[0];
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private record PlanSeedDto(
        string Name, string NameAr, string Description, string DescriptionAr,
        decimal MonthlyFee, decimal YearlyFee, decimal SetupFee,
        int MaxBranches, int MaxStaff, int MaxPatientsPerMonth,
        int MaxAppointmentsPerMonth, int MaxInvoicesPerMonth, int StorageLimitGB,
        bool HasInventoryManagement, bool HasReporting, bool HasAdvancedReporting,
        bool HasApiAccess, bool HasMultipleBranches, bool HasCustomBranding,
        bool HasPrioritySupport, bool HasBackupAndRestore, bool HasIntegrations,
        bool IsActive, bool IsPopular, int DisplayOrder);
}
