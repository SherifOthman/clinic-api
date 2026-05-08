using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Dashboard.Queries;

public class GetUsageMetricsHandler : IRequestHandler<GetUsageMetricsQuery, Result<UsageMetricsDto>>
{
    private readonly IClinicSubscriptionRepository _subscriptions;
    private readonly ICurrentUserService           _currentUser;

    public GetUsageMetricsHandler(IClinicSubscriptionRepository subscriptions, ICurrentUserService currentUser)
    {
        _subscriptions = subscriptions;
        _currentUser   = currentUser;
    }

    public async Task<Result<UsageMetricsDto>> Handle(
        GetUsageMetricsQuery request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUser.GetRequiredClinicId();

        var metrics = await _subscriptions.GetTodayMetricsAsync(clinicId, cancellationToken);
        var plan    = await _subscriptions.GetActivePlanLimitsAsync(clinicId, cancellationToken);

        return Result.Success(new UsageMetricsDto(
            Patients:     new UsageLimitDto(metrics?.NewPatientsCount   ?? 0, plan?.MaxPatientsPerMonth     ?? 0, metrics?.LastAggregatedAt),
            Appointments: new UsageLimitDto(metrics?.AppointmentsCount  ?? 0, plan?.MaxAppointmentsPerMonth ?? 0, metrics?.LastAggregatedAt),
            Invoices:     new UsageLimitDto(metrics?.InvoicesCount       ?? 0, plan?.MaxInvoicesPerMonth     ?? 0, metrics?.LastAggregatedAt),
            Staff:        new UsageLimitDto(metrics?.ActiveStaffCount    ?? 0, plan?.MaxStaff               ?? 0, metrics?.LastAggregatedAt),
            LastAggregatedAt: metrics?.LastAggregatedAt
        ));
    }
}
