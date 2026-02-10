using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;

namespace ClinicManagement.Application.Features.Onboarding.Queries.GetSubscriptionPlans;

public record GetSubscriptionPlansQuery : IRequest<Result<IEnumerable<SubscriptionPlanDto>>>;

public class GetSubscriptionPlansQueryHandler : IRequestHandler<GetSubscriptionPlansQuery, Result<IEnumerable<SubscriptionPlanDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetSubscriptionPlansQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IEnumerable<SubscriptionPlanDto>>> Handle(GetSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _unitOfWork.Repository<SubscriptionPlan>()
            .GetAllAsync(cancellationToken);
        
        var activePlans = plans.Where(p => p.IsActive).OrderBy(p => p.DisplayOrder);
        var planDtos = activePlans.Adapt<IEnumerable<SubscriptionPlanDto>>();
        
        return Result<IEnumerable<SubscriptionPlanDto>>.Ok(planDtos);
    }
}