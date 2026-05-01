using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace ClinicManagement.Application.Features.Dashboard.Queries;

public class GetDashboardStatsHandler : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMemoryCache _cache;
    private readonly ICurrentUserService _currentUser;

    public GetDashboardStatsHandler(IUnitOfWork uow, IMemoryCache cache, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _cache       = cache;
        _currentUser = currentUser;
    }

    public async Task<Result<DashboardStatsDto>> Handle(
        GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        // Cache per clinic — dashboard stats don't need to be real-time
        var clinicId  = _currentUser.GetRequiredClinicId();
        var cacheKey  = $"dashboard:stats:{clinicId}";

        if (_cache.TryGetValue(cacheKey, out DashboardStatsDto? cached) && cached is not null)
            return Result.Success(cached);

        var now       = DateTimeOffset.UtcNow;
        var thisMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var lastMonth = thisMonth.AddMonths(-1);

        // Run sequentially — DbContext is not thread-safe; parallel queries on the same
        // instance cause "A second operation was started" errors. The cache above means
        // this code path runs at most once every 5 minutes per clinic anyway.
        var totalPatients      = await _uow.Patients.CountAsync(cancellationToken);
        var patientsThisMonth  = await _uow.Patients.CountCreatedFromAsync(thisMonth, cancellationToken);
        var patientsLastMonth  = await _uow.Patients.CountCreatedBetweenAsync(lastMonth, thisMonth, cancellationToken);
        var activeStaff        = await _uow.Members.CountActiveAsync(cancellationToken);
        var pendingInvitations = await _uow.Invitations.CountPendingAsync(cancellationToken);
        var sub                = await _uow.ClinicSubscriptions.GetLatestAsync(cancellationToken);

        SubscriptionInfoDto? subscription = null;
        if (sub is not null)
        {
            var isTrial       = sub.Status == SubscriptionStatus.Trial;
            var expiryDate    = isTrial ? sub.TrialEndDate : sub.EndDate;
            var daysRemaining = expiryDate.HasValue
                ? (int?)Math.Max(0, (expiryDate.Value - now).TotalDays)
                : null;

            subscription = new SubscriptionInfoDto(
                PlanName:      sub.PlanName ?? "Unknown",
                Status:        sub.Status.ToString(),
                DaysRemaining: daysRemaining,
                IsTrial:       isTrial);
        }

        var result = new DashboardStatsDto(
            TotalPatients:      totalPatients,
            PatientsThisMonth:  patientsThisMonth,
            PatientsLastMonth:  patientsLastMonth,
            ActiveStaff:        activeStaff,
            PendingInvitations: pendingInvitations,
            Subscription:       subscription);

        // Cache for 5 minutes — stats don't need to be real-time
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return Result.Success(result);
    }
}
