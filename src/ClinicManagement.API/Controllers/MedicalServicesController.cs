using ClinicManagement.API.Extensions;
using ClinicManagement.API.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.MedicalServices.Commands.CreateMedicalService;
using ClinicManagement.Application.Features.MedicalServices.Queries.GetMedicalServices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/medical-services")]
[Authorize]
public class MedicalServicesController : BaseApiController
{
    private readonly IMediator _mediator;

    public MedicalServicesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MedicalServiceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMedicalServices(
        [FromQuery] Guid clinicBranchId,
        CancellationToken cancellationToken)
    {
        var query = new GetMedicalServicesQuery(clinicBranchId);
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMedicalService(
        [FromBody] CreateMedicalServiceCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result.ToApiError());
            
        return Created($"/api/medical-services", result.Value);
    }

    // TODO: Implement UpdateMedicalServiceCommand
}
