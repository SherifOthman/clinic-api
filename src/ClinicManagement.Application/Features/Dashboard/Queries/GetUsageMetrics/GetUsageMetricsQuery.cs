using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Dashboard.Queries;

public record GetUsageMetricsQuery : IRequest<Result<UsageMetricsDto>>;

public record UsageLimitDto(int Used, int Max, DateTimeOffset? LastUpdatedAt);

public record UsageMetricsDto(
    UsageLimitDto Patients,
    UsageLimitDto Appointments,
    UsageLimitDto Invoices,
    UsageLimitDto Staff,
    DateTimeOffset? LastAggregatedAt
);
