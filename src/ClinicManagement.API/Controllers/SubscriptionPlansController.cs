using ClinicManagement.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/subscription-plans")]
public class SubscriptionPlansController : ControllerBase
{
    /// <summary>
    /// Get all available subscription plans
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptionPlans(CancellationToken cancellationToken)
    {
        // TODO: Implement proper subscription plans from database
        // For now, return hardcoded plans until subscription system is implemented
        var plans = new List<SubscriptionPlanDto>
        {
            new()
            {
                Id = 1,
                Name = "Starter",
                Price = 29.99m,
                DoctorLimit = 2,
                AppointmentLimit = 100,
                DurationDays = 30
            },
            new()
            {
                Id = 2,
                Name = "Professional", 
                Price = 59.99m,
                DoctorLimit = 5,
                AppointmentLimit = 500,
                DurationDays = 30
            },
            new()
            {
                Id = 3,
                Name = "Enterprise",
                Price = 99.99m,
                DoctorLimit = 20,
                AppointmentLimit = 2000,
                DurationDays = 30
            }
        };

        return Ok(plans);
    }
}