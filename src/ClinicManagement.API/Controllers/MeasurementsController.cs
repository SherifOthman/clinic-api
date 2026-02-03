using ClinicManagement.API.Controllers;
using ClinicManagement.Application.Features.Measurements.Commands.CreateMeasurementAttribute;
using ClinicManagement.Application.Features.Measurements.Queries.GetMeasurementAttributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
public class MeasurementsController : BaseApiController
{
    public MeasurementsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet("attributes")]
    public async Task<IActionResult> GetMeasurementAttributes()
    {
        var result = await Mediator.Send(new GetMeasurementAttributesQuery());
        return HandleResult(result);
    }

    [HttpPost("attributes")]
    public async Task<IActionResult> CreateMeasurementAttribute(CreateMeasurementAttributeCommand command)
    {
        var result = await Mediator.Send(command);
        return HandleCreateResult(result, nameof(GetMeasurementAttributes));
    }
}
