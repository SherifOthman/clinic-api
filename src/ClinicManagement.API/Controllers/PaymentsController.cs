using ClinicManagement.API.Extensions;
using ClinicManagement.API.Models;
using ClinicManagement.Application.Features.Payments.Commands.CreatePayment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class PaymentsController : BaseApiController
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePayment(CreatePaymentCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result.ToApiError());
            
        return Created($"/api/payments/{result.Value}", result.Value);
    }
}
