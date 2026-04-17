using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;
using MediatR;

namespace ClinicManagement.Application.Features.Dashboard.Queries;

public class GetDashboardStatsHandler : IRequestHandler<GetDashboardStatsQuery, Result<DashboardStatsDto>>
{
    private readonly IUnitOfWork _uow;

    public GetDashboardStatsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<DashboardStatsDto>> Handle(
        GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var now       = DateTimeOffset.UtcNow;
        var thisMonth = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var lastMonth = thisMonth.AddMonths(-1);

        var totalPatients      = await _uow.Patients.CountAsync(cancellationToken);
        var patientsThisMonth  = await _uow.Patients.CountCreatedFromAsync(thisMonth, cancellationToken);
        var patientsLastMonth  = await _uow.Patients.CountCreatedBetweenAsync(lastMonth, thisMonth, cancellationToken);
        var activeStaff        = await _uow.Members.CountActiveAsync(cancellationToken);
        var pendingInvitations = await _uow.Invitations.CountPendingAsync(cancellationToken);

        var sub = await _uow.ClinicSubscriptions.GetLatestAsync(cancellationToken);

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

        return Result.Success(new DashboardStatsDto(
            TotalPatients:      totalPatients,
            PatientsThisMonth:  patientsThisMonth,
            PatientsLastMonth:  patientsLastMonth,
            ActiveStaff:        activeStaff,
            PendingInvitations: pendingInvitations,
            Subscription:       subscription));
    }
}
