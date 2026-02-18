using ClinicManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/subscription-plans")]
[Produces("application/json")]
public class SubscriptionPlansController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public SubscriptionPlansController(ApplicationDbContext db)
    {
        _db = db;
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
        var plans = await _db.SubscriptionPlans
            .Where(sp => sp.IsActive)
            .OrderBy(sp => sp.MonthlyFee)
            .Select(sp => new SubscriptionPlanDto(
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
            ))
            .ToListAsync(ct);

        return Ok(plans);
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
