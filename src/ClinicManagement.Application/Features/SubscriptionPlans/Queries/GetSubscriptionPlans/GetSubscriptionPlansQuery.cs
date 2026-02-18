using MediatR;

namespace ClinicManagement.Application.Features.SubscriptionPlans.Queries.GetSubscriptionPlans;

public record GetSubscriptionPlansQuery : IRequest<List<SubscriptionPlanDto>>;

public record SubscriptionPlanDto(
    int Id,
    string Name,
    string Description,
    decimal MonthlyFee,
    decimal YearlyFee,
    int MaxBranches,
    int MaxStaff,
    bool HasInventoryManagement,
    bool HasReporting,
    bool IsActive);
