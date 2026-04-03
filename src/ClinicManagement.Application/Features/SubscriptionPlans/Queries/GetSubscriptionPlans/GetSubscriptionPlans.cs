using ClinicManagement.Application.Abstractions.Data;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ClinicManagement.Application.Features.SubscriptionPlans.Queries;

public record GetSubscriptionPlansQuery : IRequest<IEnumerable<SubscriptionPlanDto>>;

public record SubscriptionPlanDto(
    Guid Id,
    string Name,
    string NameAr,
    string Description,
    string DescriptionAr,
    decimal MonthlyFee,
    decimal YearlyFee,
    decimal SetupFee,
    int MaxBranches,
    int MaxStaff,
    int MaxPatientsPerMonth,
    int MaxAppointmentsPerMonth,
    int MaxInvoicesPerMonth,
    int StorageLimitGB,
    bool HasInventoryManagement,
    bool HasReporting,
    bool HasAdvancedReporting,
    bool HasApiAccess,
    bool HasMultipleBranches,
    bool HasCustomBranding,
    bool HasPrioritySupport,
    bool HasBackupAndRestore,
    bool HasIntegrations,
    bool IsActive,
    bool IsPopular,
    int DisplayOrder,
    int Version,
    DateTime EffectiveDate,
    DateTime? ExpiryDate);

public class GetSubscriptionPlansHandler : IRequestHandler<GetSubscriptionPlansQuery, IEnumerable<SubscriptionPlanDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    private const string CacheKey = "subscription_plans";

    public GetSubscriptionPlansHandler(IApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<IEnumerable<SubscriptionPlanDto>> Handle(GetSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        return await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var plans = await _context.SubscriptionPlans
                .Where(p => p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync(cancellationToken);
            return plans.Adapt<List<SubscriptionPlanDto>>().AsEnumerable();
        }) ?? Enumerable.Empty<SubscriptionPlanDto>();
    }
}
