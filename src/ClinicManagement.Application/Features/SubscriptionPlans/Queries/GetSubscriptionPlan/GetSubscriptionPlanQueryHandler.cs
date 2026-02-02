using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.SubscriptionPlans.Queries.GetSubscriptionPlan;

public class GetSubscriptionPlanQueryHandler : IRequestHandler<GetSubscriptionPlanQuery, Result<SubscriptionPlanDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetSubscriptionPlanQueryHandler> _logger;

    public GetSubscriptionPlanQueryHandler(IUnitOfWork unitOfWork, ILogger<GetSubscriptionPlanQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<SubscriptionPlanDto>> Handle(GetSubscriptionPlanQuery request, CancellationToken cancellationToken)
    {
        var subscriptionPlan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(request.Id, cancellationToken);
        
        if (subscriptionPlan == null)
        {
            _logger.LogWarning("Subscription plan with ID {Id} not found", request.Id);
            return Result<SubscriptionPlanDto>.Fail(MessageCodes.Business.SUBSCRIPTION_PLAN_NOT_FOUND);
        }

        var dto = subscriptionPlan.Adapt<SubscriptionPlanDto>();
        return Result<SubscriptionPlanDto>.Ok(dto);
    }
}
