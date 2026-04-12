using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Dashboard.Queries;

public record GetDashboardStatsQuery : IRequest<Result<DashboardStatsDto>>;

public record DashboardStatsDto(
    int TotalPatients,
    int PatientsThisMonth,
    int PatientsLastMonth,
    int ActiveStaff,
    int PendingInvitations,
    SubscriptionInfoDto? Subscription
);

public record SubscriptionInfoDto(
    string PlanName,
    string Status,
    int? DaysRemaining,
    bool IsTrial
);
