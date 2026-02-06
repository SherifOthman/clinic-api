using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.ChronicDiseases.Commands.CreateChronicDisease;
using ClinicManagement.Application.Features.ChronicDiseases.Commands.DeleteChronicDisease;
using ClinicManagement.Application.Features.ChronicDiseases.Commands.UpdateChronicDisease;
using ClinicManagement.Application.Features.ChronicDiseases.Queries.GetChronicDisease;
using ClinicManagement.Application.Features.ChronicDiseases.Queries.GetChronicDiseases;
using ClinicManagement.Application.Features.ChronicDiseases.Queries.GetChronicDiseasesWithPagination;
using ClinicManagement.Domain.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ChronicDiseasesController : BaseApiController
{
    private readonly IMediator _mediator;

    public ChronicDiseasesController(IMediator mediator) { _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetChronicDiseases([FromQuery] string? language = null)
    {
        var result = await _mediator.Send(new GetChronicDiseasesQuery(language));
        return HandleResult(result);
    }

    [HttpGet("paginated")]
    [ProducesResponseType(typeof(PagedResult<ChronicDiseaseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChronicDiseasesWithPagination(
        [FromQuery] GetChronicDiseasesWithPaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ChronicDiseaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChronicDisease(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetChronicDiseaseQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ChronicDiseaseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateChronicDisease(CreateChronicDiseaseCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return HandleCreateResult(result, nameof(GetChronicDisease), new { id = result.Value?.Id });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ChronicDiseaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateChronicDisease(Guid id, UpdateChronicDiseaseCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(MessageCodes.Controller.ID_MISMATCH);

        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteChronicDisease(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteChronicDiseaseCommand { Id = id };
        var result = await _mediator.Send(command, cancellationToken);
        return HandleDeleteResult(result);
    }
}
