using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.SubscriptionPlans.Commands.CreateSubscriptionPlan;

public class CreateSubscriptionPlanCommandHandler : IRequestHandler<CreateSubscriptionPlanCommand, Result<SubscriptionPlanDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateSubscriptionPlanCommandHandler> _logger;

    public CreateSubscriptionPlanCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateSubscriptionPlanCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<SubscriptionPlanDto>> Handle(CreateSubscriptionPlanCommand request, CancellationToken cancellationToken)
    {
        var subscriptionPlan = request.Adapt<SubscriptionPlan>();
        
        await _unitOfWork.SubscriptionPlans.AddAsync(subscriptionPlan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = subscriptionPlan.Adapt<SubscriptionPlanDto>();
        
        _logger.LogInformation("Created subscription plan with ID {Id}", subscriptionPlan.Id);
        
        return Result<SubscriptionPlanDto>.Ok(dto);
    }
}
