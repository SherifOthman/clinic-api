using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Application.Features.SubscriptionPlans.QueryModels;

public record ClinicSubscriptionRow(
    SubscriptionStatus Status,
    DateTimeOffset? TrialEndDate,
    DateTimeOffset? EndDate,
    string? PlanName
);
