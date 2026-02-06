using ClinicManagement.Application.Features.Specializations.Queries.GetSpecializations;
using ClinicManagement.Application.Features.Specializations.Queries.GetSpecializationById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpecializationsController : BaseApiController
{
    private readonly IMediator _mediator;

    public SpecializationsController(IMediator mediator) 
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetSpecializations()
    {
        var query = new GetSpecializationsQuery();
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSpecialization(Guid id)
    {
        var query = new GetSpecializationByIdQuery(id);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }
}
