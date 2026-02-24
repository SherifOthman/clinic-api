using ClinicManagement.Domain.Repositories;
using Mapster;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace ClinicManagement.Application.SubscriptionPlans.Queries;

public record GetSubscriptionPlansQuery : IRequest<IEnumerable<SubscriptionPlanDto>>;

public record SubscriptionPlanDto(
    Guid Id,
    string Name,
    string Description,
    decimal MonthlyFee,
    decimal YearlyFee,
    int MaxBranches,
    int MaxStaff,
    bool HasInventoryManagement,
    bool HasReporting,
    bool IsActive);

public class GetSubscriptionPlansHandler : IRequestHandler<GetSubscriptionPlansQuery, IEnumerable<SubscriptionPlanDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    private const string CacheKey = "subscription_plans";

    public GetSubscriptionPlansHandler(IUnitOfWork unitOfWork, IMemoryCache cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<IEnumerable<SubscriptionPlanDto>> Handle(GetSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        return await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            var plans = await _unitOfWork.SubscriptionPlans.GetActiveAsync(cancellationToken);
            return plans.Adapt<List<SubscriptionPlanDto>>().AsEnumerable();
        }) ?? Enumerable.Empty<SubscriptionPlanDto>();
    }
}
