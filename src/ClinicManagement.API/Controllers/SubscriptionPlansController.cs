using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.SubscriptionPlans.Commands.CreateSubscriptionPlan;
using ClinicManagement.Application.Features.SubscriptionPlans.Commands.DeleteSubscriptionPlan;
using ClinicManagement.Application.Features.SubscriptionPlans.Commands.UpdateSubscriptionPlan;
using ClinicManagement.Application.Features.SubscriptionPlans.Queries.GetSubscriptionPlan;
using ClinicManagement.Application.Features.SubscriptionPlans.Queries.GetSubscriptionPlans;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class SubscriptionPlansController : BaseApiController
{
    public SubscriptionPlansController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<SubscriptionPlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSubscriptionPlans(CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetSubscriptionPlansQuery(), cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SubscriptionPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubscriptionPlan(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetSubscriptionPlanQuery(id);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(SubscriptionPlanDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateSubscriptionPlan(CreateSubscriptionPlanCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleCreateResult(result, nameof(GetSubscriptionPlan), new { id = result.Value?.Id });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(SubscriptionPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSubscriptionPlan(Guid id, UpdateSubscriptionPlanCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(MessageCodes.Controller.ID_MISMATCH);

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSubscriptionPlan(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteSubscriptionPlanCommand(id);
        var result = await Mediator.Send(command, cancellationToken);
        return HandleDeleteResult(result);
    }
}