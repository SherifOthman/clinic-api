using ClinicManagement.API.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.ChronicDiseases.Queries.GetChronicDiseases;
using ClinicManagement.Application.Features.ChronicDiseases.Queries.GetChronicDiseasesPaginated;
using ClinicManagement.Domain.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/chronic-diseases")]
[AllowAnonymous]
public class ChronicDiseasesController : BaseApiController
{
    private readonly IMediator _mediator;

    public ChronicDiseasesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ChronicDiseaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetChronicDiseases([FromQuery] string? language, CancellationToken cancellationToken)
    {
        var query = new GetChronicDiseasesQuery(language);
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("paginated")]
    [ProducesResponseType(typeof(PagedResult<ChronicDiseaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetChronicDiseasesWithPagination(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetChronicDiseasesPaginatedQuery(pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    // Note: GetById endpoint removed as it was accessing DbContext directly
    // If needed, create GetChronicDiseaseByIdQuery + Handler
}
