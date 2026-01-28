using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.ChronicDiseases.Commands.CreateChronicDisease;
using ClinicManagement.Application.Features.ChronicDiseases.Commands.DeleteChronicDisease;
using ClinicManagement.Application.Features.ChronicDiseases.Commands.UpdateChronicDisease;
using ClinicManagement.Application.Features.ChronicDiseases.Queries.GetChronicDisease;
using ClinicManagement.Application.Features.ChronicDiseases.Queries.GetChronicDiseases;
using ClinicManagement.Application.Features.ChronicDiseases.Queries.GetChronicDiseasesWithPagination;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class ChronicDiseasesController : BaseApiController
{
    public ChronicDiseasesController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [Authorize(Roles = RoleNames.AllStaff)]
    public async Task<IActionResult> GetChronicDiseases()
    {
        var result = await Mediator.Send(new GetChronicDiseasesQuery());
        return HandleResult(result);
    }

    [HttpGet("paginated")]
    [Authorize(Roles = RoleNames.AllStaff)]
    [ProducesResponseType(typeof(PagedResult<ChronicDiseaseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChronicDiseasesWithPagination(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetChronicDiseasesWithPaginationQuery(pageNumber, pageSize, searchTerm, sortBy, sortDescending, isActive);
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = RoleNames.AllStaff)]
    [ProducesResponseType(typeof(ChronicDiseaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChronicDisease(int id, CancellationToken cancellationToken)
    {
        var query = new GetChronicDiseaseQuery { Id = id };
        var result = await Mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.AdminAndManagement)]
    [ProducesResponseType(typeof(ChronicDiseaseDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateChronicDisease(CreateChronicDiseaseCommand command, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);
        return HandleCreateResult(result, nameof(GetChronicDisease), new { id = result.Value?.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = RoleNames.AdminAndManagement)]
    [ProducesResponseType(typeof(ChronicDiseaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateChronicDisease(int id, UpdateChronicDiseaseCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");

        var result = await Mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = RoleNames.AdminAndManagement)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteChronicDisease(int id, CancellationToken cancellationToken)
    {
        var command = new DeleteChronicDiseaseCommand { Id = id };
        var result = await Mediator.Send(command, cancellationToken);
        return HandleDeleteResult(result);
    }
}