using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Features.Reference.QueryModels;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ClinicManagement.Persistence.Repositories;

public class ReferenceRepository : IReferenceRepository
{
    private readonly DbSet<ChronicDisease>   _chronicDiseases;
    private readonly DbSet<Specialization>   _specializations;
    private readonly DbSet<SubscriptionPlan> _subscriptionPlans;
    private readonly IMemoryCache            _cache;
    private static readonly TimeSpan         CacheDuration = TimeSpan.FromHours(24);

    public ReferenceRepository(ApplicationDbContext context, IMemoryCache cache)
    {
        _chronicDiseases   = context.Set<ChronicDisease>();
        _specializations   = context.Set<Specialization>();
        _subscriptionPlans = context.Set<SubscriptionPlan>();
        _cache             = cache;
    }

    public async Task<List<ChronicDiseaseRow>> GetChronicDiseasesAsync(CancellationToken ct = default)
        => await _cache.GetOrCreateAsync("ref:chronic_diseases", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await _chronicDiseases
                .OrderBy(d => d.NameEn)
                .Select(d => new ChronicDiseaseRow(d.Id, d.NameEn, d.NameAr))
                .ToListAsync(ct);
        }) ?? [];

    public async Task<List<SpecializationRow>> GetSpecializationsAsync(CancellationToken ct = default)
        => await _cache.GetOrCreateAsync("ref:specializations", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await _specializations
                .OrderBy(s => s.NameEn)
                .Select(s => new SpecializationRow(s.Id, s.NameEn, s.NameAr, s.DescriptionEn, s.DescriptionAr))
                .ToListAsync(ct);
        }) ?? [];

    public async Task<List<SubscriptionPlanRow>> GetActiveSubscriptionPlansAsync(CancellationToken ct = default)
        => await _cache.GetOrCreateAsync("ref:subscription_plans", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await _subscriptionPlans
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .Select(p => new SubscriptionPlanRow(
                    p.Id, p.Name, p.NameAr, p.Description, p.DescriptionAr,
                    p.MonthlyFee, p.YearlyFee, p.SetupFee,
                    p.MaxBranches, p.MaxStaff, p.MaxPatientsPerMonth,
                    p.MaxAppointmentsPerMonth, p.MaxInvoicesPerMonth, (int)p.StorageLimitGB,
                    p.HasInventoryManagement, p.HasReporting, p.HasAdvancedReporting,
                    p.HasApiAccess, p.HasMultipleBranches, p.HasCustomBranding,
                    p.HasPrioritySupport, p.HasBackupAndRestore, p.HasIntegrations,
                    p.IsActive, p.IsPopular, p.DisplayOrder,
                    p.Version, p.EffectiveDate, p.ExpiryDate))
                .ToListAsync(ct);
        }) ?? [];

    public async Task<bool> SpecializationExistsAsync(Guid id, CancellationToken ct = default)
        => await _specializations.AnyAsync(s => s.Id == id, ct);

    public async Task<bool> SubscriptionPlanExistsAsync(Guid id, CancellationToken ct = default)
        => await _subscriptionPlans.AnyAsync(p => p.Id == id, ct);
}
