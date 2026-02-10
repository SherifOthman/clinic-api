using ClinicManagement.API.Extensions;
using ClinicManagement.API.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.MedicalSupplies.Commands.CreateMedicalSupply;
using ClinicManagement.Application.Features.MedicalSupplies.Queries.GetMedicalSupplies;
using ClinicManagement.Domain.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/medical-supplies")]
[Authorize]
public class MedicalSuppliesController : BaseApiController
{
    private readonly IMediator _mediator;

    public MedicalSuppliesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<MedicalSupplyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMedicalSupplies(
        [FromQuery] Guid clinicBranchId,
        [FromQuery] int? pageNumber,
        [FromQuery] int? pageSize,
        CancellationToken cancellationToken)
    {
        var query = new GetMedicalSuppliesQuery(clinicBranchId, pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMedicalSupply(
        [FromBody] CreateMedicalSupplyCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result.ToApiError());
            
        return Created($"/api/medical-supplies", result.Value);
    }

    // TODO: Implement UpdateMedicalSupplyCommand
}
