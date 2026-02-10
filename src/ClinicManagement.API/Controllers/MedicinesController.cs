using ClinicManagement.API.Extensions;
using ClinicManagement.API.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Medicines.Queries.GetMedicineById;
using ClinicManagement.Application.Features.Medicines.Queries.GetMedicines;
using ClinicManagement.Domain.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/medicines")]
[Authorize]
public class MedicinesController : BaseApiController
{
    private readonly IMediator _mediator;

    public MedicinesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<MedicineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMedicines(
        [FromQuery] Guid clinicBranchId,
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var query = new GetMedicinesQuery(clinicBranchId, pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MedicineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMedicineById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetMedicineByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (!result.Success)
            return NotFound(result.ToApiError());
            
        return Ok(result.Value);
    }
}
