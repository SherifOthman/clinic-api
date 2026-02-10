using ClinicManagement.API.Extensions;
using ClinicManagement.API.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Specializations.Queries.GetSpecializationById;
using ClinicManagement.Application.Features.Specializations.Queries.GetSpecializations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/specializations")]
[Authorize]
public class SpecializationsController : BaseApiController
{
    private readonly IMediator _mediator;

    public SpecializationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SpecializationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSpecializations(CancellationToken cancellationToken)
    {
        var query = new GetSpecializationsQuery();
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SpecializationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSpecializationById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetSpecializationByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        
        if (!result.Success)
            return NotFound(result.ToApiError());
            
        return Ok(result.Value);
    }
}
