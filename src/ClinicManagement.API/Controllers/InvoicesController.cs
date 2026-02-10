using ClinicManagement.API.Extensions;
using ClinicManagement.API.Models;
using ClinicManagement.Application.Features.Invoices.Commands.CreateInvoice;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class InvoicesController : BaseApiController
{
    private readonly IMediator _mediator;

    public InvoicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInvoice(CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result.ToApiError());
            
        return Created($"/api/invoices", result.Value);
    }

    // Note: GetInvoices endpoint removed as it was accessing DbContext directly
    // If needed, create GetInvoicesQuery + Handler
}
