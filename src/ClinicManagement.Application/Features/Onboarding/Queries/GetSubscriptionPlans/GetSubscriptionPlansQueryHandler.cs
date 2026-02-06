using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Onboarding.Queries.GetSubscriptionPlans;

public class GetSubscriptionPlansQueryHandler : IRequestHandler<GetSubscriptionPlansQuery, Result<IEnumerable<SubscriptionPlanDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSubscriptionPlansQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<SubscriptionPlanDto>>> Handle(GetSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _unitOfWork.SubscriptionPlans.GetActiveAsync(cancellationToken);
        var planDtos = plans.Adapt<IEnumerable<SubscriptionPlanDto>>();
        return Result<IEnumerable<SubscriptionPlanDto>>.Ok(planDtos);
    }
}
