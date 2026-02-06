using ClinicManagement.API.Controllers;
using ClinicManagement.Application.Features.Medicines.Commands.CreateMedicine;
using ClinicManagement.Application.Features.Medicines.Commands.DeleteMedicine;
using ClinicManagement.Application.Features.Medicines.Commands.UpdateMedicine;
using ClinicManagement.Application.Features.Medicines.Queries.GetMedicine;
using ClinicManagement.Application.Features.Medicines.Queries.GetMedicines;
using ClinicManagement.Domain.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
public class MedicinesController : BaseApiController
{
    private readonly IMediator _mediator;

    public MedicinesController(IMediator mediator) { _mediator = mediator;
    }

    [HttpGet("branch/{branchId}")]
    public async Task<IActionResult> GetMedicines(
        Guid branchId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string sortDirection = "asc",
        [FromQuery] bool? isLowStock = null)
    {
        var paginationRequest = new SearchablePaginationRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        if (isLowStock.HasValue)
        {
            paginationRequest.Filters.Add("isLowStock", isLowStock.Value);
        }

        var result = await _mediator.Send(new GetMedicinesQuery(branchId, paginationRequest));
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetMedicine(Guid id)
    {
        var result = await _mediator.Send(new GetMedicineQuery(id));
        return HandleResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMedicine(CreateMedicineCommand command)
    {
        var result = await _mediator.Send(command);
        return HandleCreateResult(result, nameof(GetMedicine), new { id = result.Value });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMedicine(Guid id, UpdateMedicineCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("ID mismatch");
        }

        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMedicine(Guid id)
    {
        var result = await _mediator.Send(new DeleteMedicineCommand(id));
        return HandleDeleteResult(result);
    }
}
