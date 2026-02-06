using ClinicManagement.API.Controllers;
using ClinicManagement.Application.Features.Invoices.Commands.CreateInvoice;
using ClinicManagement.Application.Features.Invoices.Queries.GetInvoices;
using ClinicManagement.Domain.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
public class InvoicesController : BaseApiController
{
    private readonly IMediator _mediator;

    public InvoicesController(IMediator mediator) { _mediator = mediator;
    }

    [HttpGet("clinic/{clinicId}")]
    public async Task<IActionResult> GetInvoices(
        Guid clinicId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string sortDirection = "desc",
        [FromQuery] string? status = null,
        [FromQuery] bool? isOverdue = null)
    {
        var paginationRequest = new SearchablePaginationRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        if (!string.IsNullOrEmpty(status))
        {
            paginationRequest.Filters.Add("status", status);
        }

        if (isOverdue.HasValue)
        {
            paginationRequest.Filters.Add("isOverdue", isOverdue.Value);
        }

        var result = await _mediator.Send(new GetInvoicesQuery(clinicId, paginationRequest));
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateInvoice(CreateInvoiceCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleCreateResult(result, nameof(GetInvoices), new { clinicId = command.PatientId }); // Use patient ID as placeholder
    }
}
