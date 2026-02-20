using ClinicManagement.Domain.Repositories;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.SubscriptionPlans.Queries;

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

public class GetSubscriptionPlansHandler : IRequestHandler<GetSubscriptionPlansQuery, List<SubscriptionPlanDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSubscriptionPlansHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<SubscriptionPlanDto>> Handle(GetSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _unitOfWork.SubscriptionPlans.GetActiveAsync(cancellationToken);
        return plans.Adapt<List<SubscriptionPlanDto>>();
    }
}
