using ClinicManagement.API.Extensions;
using ClinicManagement.API.Models;
using ClinicManagement.Application.Features.Measurements.Commands.CreateMeasurementAttribute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class MeasurementsController : BaseApiController
{
    private readonly IMediator _mediator;

    public MeasurementsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("attributes")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMeasurementAttribute(CreateMeasurementAttributeCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result.ToApiError());
            
        return Created($"/api/measurements/attributes", result.Value);
    }

    // Note: GetMeasurementAttributes endpoint removed as it was accessing DbContext directly
    // If needed, create GetMeasurementAttributesQuery + Handler
}
