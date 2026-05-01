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

        // Run all COUNT queries in parallel — they are independent
        var totalPatientsTask      = _uow.Patients.CountAsync(cancellationToken);
        var patientsThisMonthTask  = _uow.Patients.CountCreatedFromAsync(thisMonth, cancellationToken);
        var patientsLastMonthTask  = _uow.Patients.CountCreatedBetweenAsync(lastMonth, thisMonth, cancellationToken);
        var activeStaffTask        = _uow.Members.CountActiveAsync(cancellationToken);
        var pendingInvitationsTask = _uow.Invitations.CountPendingAsync(cancellationToken);
        var subTask                = _uow.ClinicSubscriptions.GetLatestAsync(cancellationToken);

        await Task.WhenAll(
            totalPatientsTask, patientsThisMonthTask, patientsLastMonthTask,
            activeStaffTask, pendingInvitationsTask, subTask);

        var sub = subTask.Result;
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
            TotalPatients:      totalPatientsTask.Result,
            PatientsThisMonth:  patientsThisMonthTask.Result,
            PatientsLastMonth:  patientsLastMonthTask.Result,
            ActiveStaff:        activeStaffTask.Result,
            PendingInvitations: pendingInvitationsTask.Result,
            Subscription:       subscription);

        // Cache for 5 minutes — stats don't need to be real-time
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return Result.Success(result);
    }
}
