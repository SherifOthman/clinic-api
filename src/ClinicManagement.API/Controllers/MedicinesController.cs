using ClinicManagement.API.Extensions;
using ClinicManagement.API.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Medicines.Commands.CreateMedicine;
using ClinicManagement.Application.Features.Medicines.Queries.GetMedicines;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/medicines")]
[Authorize]
public class MedicinesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MedicinesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Produces("application/json")]
    [ProducesResponseType(typeof(List<MedicineDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMedicines([FromQuery] string? searchTerm, CancellationToken cancellationToken)
    {
        var query = new GetMedicinesQuery { SearchTerm = searchTerm };
        var result = await _mediator.Send(query, cancellationToken);

        return result.Success
            ? Ok(result.Value)
            : BadRequest(result.ToApiError());
    }

    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType(typeof(MedicineDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMedicine(CreateMedicineCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return result.Success
            ? Created(string.Empty, result.Value)
            : BadRequest(result.ToApiError());
    }
}
