using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.SubscriptionPlans.Commands.DeleteSubscriptionPlan;

public class DeleteSubscriptionPlanCommandHandler : IRequestHandler<DeleteSubscriptionPlanCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteSubscriptionPlanCommandHandler> _logger;

    public DeleteSubscriptionPlanCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteSubscriptionPlanCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        var subscriptionPlan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(request.Id, cancellationToken);
        
        if (subscriptionPlan == null)
        {
            _logger.LogWarning("Subscription plan with ID {Id} not found", request.Id);
            return Result.Fail(MessageCodes.Business.SUBSCRIPTION_PLAN_NOT_FOUND);
        }

        await _unitOfWork.SubscriptionPlans.DeleteAsync(subscriptionPlan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _logger.LogInformation("Deleted subscription plan with ID {Id}", request.Id);
        return Result.Ok();
    }
}
