using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace ClinicManagement.Application.Features.Dashboard.Queries;

public class GetDashboardStatsHandler : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IPatientRepository              _patients;
    private readonly IClinicMemberRepository         _members;
    private readonly IInvitationRepository           _invitations;
    private readonly IClinicSubscriptionRepository   _subscriptions;
    private readonly IMemoryCache                    _cache;
    private readonly ICurrentUserService             _currentUser;

    public GetDashboardStatsHandler(
        IPatientRepository patients,
        IClinicMemberRepository members,
        IInvitationRepository invitations,
        IClinicSubscriptionRepository subscriptions,
        IMemoryCache cache,
        ICurrentUserService currentUser)
    {
        _patients      = patients;
        _members       = members;
        _invitations   = invitations;
        _subscriptions = subscriptions;
        _cache         = cache;
        _currentUser   = currentUser;
    }

    public async Task<Result<DashboardStatsDto>> Handle(
        GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUser.GetRequiredClinicId();
        var cacheKey = $"dashboard:stats:{clinicId}";

        if (_cache.TryGetValue(cacheKey, out DashboardStatsDto? cached) && cached is not null)
            return Result.Success(cached);

        var now       = DateTimeOffset.UtcNow;
        var thisMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var lastMonth = thisMonth.AddMonths(-1);

        var totalPatients      = await _patients.CountAsync(cancellationToken);
        var patientsThisMonth  = await _patients.CountCreatedFromAsync(thisMonth, cancellationToken);
        var patientsLastMonth  = await _patients.CountCreatedBetweenAsync(lastMonth, thisMonth, cancellationToken);
        var activeStaff        = await _members.CountActiveAsync(cancellationToken);
        var pendingInvitations = await _invitations.CountPendingAsync(cancellationToken);
        var sub                = await _subscriptions.GetLatestAsync(cancellationToken);

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

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return Result.Success(result);
    }
}
