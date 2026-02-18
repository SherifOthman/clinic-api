using ClinicManagement.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/subscription-plans")]
[Produces("application/json")]
public class SubscriptionPlansController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public SubscriptionPlansController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Get all subscription plans
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [OutputCache(PolicyName = "ReferenceData")]
    [ProducesResponseType(typeof(List<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptionPlans(CancellationToken ct)
    {
        var plans = await _unitOfWork.SubscriptionPlans.GetActiveAsync(ct);

        var dtos = plans.Select(sp => new SubscriptionPlanDto(
            sp.Id,
            sp.Name,
            sp.Description,
            sp.MonthlyFee,
            sp.YearlyFee,
            sp.MaxBranches,
            sp.MaxStaff,
            sp.HasInventoryManagement,
            sp.HasReporting,
            sp.IsActive
        )).ToList();

        return Ok(dtos);
    }
}

public record SubscriptionPlanDto(
    Guid Id,
    string Name,
    string Description,
    decimal MonthlyFee,
    decimal YearlyFee,
    int MaxBranches,
    int MaxStaff,
    bool HasInventoryManagement,
    bool HasReporting,
    bool IsActive);
