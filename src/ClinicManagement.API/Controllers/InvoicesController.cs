using ClinicManagement.API.Controllers;
using ClinicManagement.Application.Features.Invoices.Commands.CreateInvoice;
using ClinicManagement.Application.Features.Invoices.Queries.GetInvoices;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
public class InvoicesController : BaseApiController
{
    [HttpGet("clinic/{clinicId}")]
    public async Task<IActionResult> GetInvoices(Guid clinicId)
    {
        var result = await Mediator.Send(new GetInvoicesQuery(clinicId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateInvoice(CreateInvoiceCommand command)
    {
        var result = await Mediator.Send(command);
        return Ok(result);
    }
}