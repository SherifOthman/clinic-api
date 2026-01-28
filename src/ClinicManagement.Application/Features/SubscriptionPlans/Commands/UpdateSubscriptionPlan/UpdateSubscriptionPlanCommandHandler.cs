using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.SubscriptionPlans.Commands.UpdateSubscriptionPlan;

public class UpdateSubscriptionPlanCommandHandler : IRequestHandler<UpdateSubscriptionPlanCommand, Result<SubscriptionPlanDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateSubscriptionPlanCommandHandler> _logger;

    public UpdateSubscriptionPlanCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateSubscriptionPlanCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<SubscriptionPlanDto>> Handle(UpdateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        var subscriptionPlan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(request.Id, cancellationToken);
        
        if (subscriptionPlan == null)
        {
            _logger.LogWarning("Subscription plan with ID {Id} not found", request.Id);
            return Result<SubscriptionPlanDto>.Fail(ApplicationErrors.Business.SUBSCRIPTION_PLAN_NOT_FOUND);
        }

        request.Adapt(subscriptionPlan);
        await _unitOfWork.SubscriptionPlans.UpdateAsync(subscriptionPlan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = subscriptionPlan.Adapt<SubscriptionPlanDto>();
        
        _logger.LogInformation("Updated subscription plan with ID {Id}", request.Id);
        return Result<SubscriptionPlanDto>.Ok(dto);
    }
}
