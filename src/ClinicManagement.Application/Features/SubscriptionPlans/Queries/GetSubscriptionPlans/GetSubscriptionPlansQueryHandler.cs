using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.SubscriptionPlans.Queries.GetSubscriptionPlans;

public class GetSubscriptionPlansQueryHandler : IRequestHandler<GetSubscriptionPlansQuery, Result<List<SubscriptionPlanDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetSubscriptionPlansQueryHandler> _logger;

    public GetSubscriptionPlansQueryHandler(IUnitOfWork unitOfWork, ILogger<GetSubscriptionPlansQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<List<SubscriptionPlanDto>>> Handle(GetSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        var activePlans = await _unitOfWork.SubscriptionPlans.GetActiveAsync(cancellationToken);
        var dtos = activePlans.Adapt<List<SubscriptionPlanDto>>();
        
        _logger.LogInformation("Retrieved {Count} active subscription plans", dtos.Count);
        
        return Result<List<SubscriptionPlanDto>>.Ok(dtos);
    }
}
