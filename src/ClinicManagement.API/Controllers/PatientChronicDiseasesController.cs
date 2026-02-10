using ClinicManagement.API.Extensions;
using ClinicManagement.API.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Patients.Commands.AddChronicDisease;
using ClinicManagement.Application.Features.Patients.Commands.RemoveChronicDisease;
using ClinicManagement.Application.Features.Patients.Commands.UpdateChronicDisease;
using ClinicManagement.Application.Features.Patients.Queries.GetPatientChronicDiseases;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[Route("api/patients/{patientId:guid}/chronic-diseases")]
[Authorize]
public class PatientChronicDiseasesController : BaseApiController
{
    private readonly IMediator _mediator;

    public PatientChronicDiseasesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PatientChronicDiseaseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientChronicDiseases(
        Guid patientId,
        CancellationToken cancellationToken)
    {
        var query = new GetPatientChronicDiseasesQuery(patientId);
        var result = await _mediator.Send(query, cancellationToken);
        return HandleResult(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PatientChronicDiseaseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddPatientChronicDisease(
        Guid patientId,
        [FromBody] CreatePatientChronicDiseaseDto createDto,
        CancellationToken cancellationToken)
    {
        var command = new AddChronicDiseaseCommand(patientId, createDto);
        var result = await _mediator.Send(command, cancellationToken);
        
        if (!result.Success)
            return BadRequest(result.ToApiError());
            
        return Created($"/api/patients/{patientId}/chronic-diseases", result.Value);
    }

    [HttpPut("{chronicDiseaseId:guid}")]
    [ProducesResponseType(typeof(PatientChronicDiseaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePatientChronicDisease(
        Guid patientId,
        Guid chronicDiseaseId,
        [FromBody] UpdatePatientChronicDiseaseDto updateDto,
        CancellationToken cancellationToken)
    {
        var command = new UpdateChronicDiseaseCommand(patientId, chronicDiseaseId, updateDto);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleResult(result);
    }

    [HttpDelete("{chronicDiseaseId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemovePatientChronicDisease(
        Guid patientId,
        Guid chronicDiseaseId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveChronicDiseaseCommand(patientId, chronicDiseaseId);
        var result = await _mediator.Send(command, cancellationToken);
        return HandleDeleteResult(result);
    }
}
