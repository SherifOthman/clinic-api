using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Dashboard.Queries;

public class GetUsageMetricsHandler : IRequestHandler<GetUsageMetricsQuery, Result<UsageMetricsDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public GetUsageMetricsHandler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<Result<UsageMetricsDto>> Handle(
        GetUsageMetricsQuery request, CancellationToken cancellationToken)
    {
        var clinicId = _currentUser.GetRequiredClinicId();

        // Today's aggregated metrics (written by UsageMetricsAggregationJob at 1am)
        var metrics = await _uow.ClinicSubscriptions.GetTodayMetricsAsync(clinicId, cancellationToken);

        // Active plan limits
        var plan = await _uow.ClinicSubscriptions.GetActivePlanLimitsAsync(clinicId, cancellationToken);

        // If no metrics yet today (job hasn't run), return zeros with null LastAggregatedAt
        // so the frontend can show "not yet available today"
        return Result.Success(new UsageMetricsDto(
            Patients:     new UsageLimitDto(metrics?.NewPatientsCount   ?? 0, plan?.MaxPatientsPerMonth     ?? 0, metrics?.LastAggregatedAt),
            Appointments: new UsageLimitDto(metrics?.AppointmentsCount  ?? 0, plan?.MaxAppointmentsPerMonth ?? 0, metrics?.LastAggregatedAt),
            Invoices:     new UsageLimitDto(metrics?.InvoicesCount       ?? 0, plan?.MaxInvoicesPerMonth     ?? 0, metrics?.LastAggregatedAt),
            Staff:        new UsageLimitDto(metrics?.ActiveStaffCount    ?? 0, plan?.MaxStaff               ?? 0, metrics?.LastAggregatedAt),
            LastAggregatedAt: metrics?.LastAggregatedAt
        ));
    }
}
