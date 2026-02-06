using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Patients.Commands.AddChronicDisease;
using ClinicManagement.Application.Features.Patients.Commands.RemoveChronicDisease;
using ClinicManagement.Application.Features.Patients.Commands.UpdateChronicDisease;
using ClinicManagement.Application.Features.Patients.Queries.GetChronicDiseases;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/patients/{PatientId}/chronic-diseases")]
public class PatientChronicDiseasesController : BaseApiController
{
    private readonly IMediator _mediator;

    public PatientChronicDiseasesController(IMediator mediator) { _mediator = mediator;
    }

    /// <summary>
    /// Get all chronic diseases for a patient
    /// </summary>
    /// <param name="PatientId">The patient ID</param>
    /// <param name="activeOnly">Whether to return only active chronic diseases</param>
    /// <returns>List of chronic diseases</returns>
    [HttpGet]
    public async Task<IActionResult> GetChronicDiseases(Guid PatientId, [FromQuery] bool activeOnly = false)
    {
        var query = new GetChronicDiseasesQuery(PatientId, activeOnly);
        var result = await _mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Add a chronic disease to a clinic patient
    /// </summary>
    /// <param name="PatientId">The clinic patient ID</param>
    /// <param name="createDto">The chronic disease data</param>
    /// <returns>The created chronic disease relationship</returns>
    [HttpPost]
    public async Task<IActionResult> AddChronicDisease(Guid PatientId, [FromBody] CreatePatientChronicDiseaseDto createDto)
    {
        var command = new AddChronicDiseaseCommand(PatientId, createDto);
        var result = await _mediator.Send(command);
        return HandleCreateResult(result, nameof(GetChronicDiseases), new { PatientId });
    }

    /// <summary>
    /// Update a chronic disease relationship for a clinic patient
    /// </summary>
    /// <param name="PatientId">The clinic patient ID</param>
    /// <param name="chronicDiseaseId">The chronic disease ID</param>
    /// <param name="updateDto">The update data</param>
    /// <returns>The updated chronic disease relationship</returns>
    [HttpPut("{chronicDiseaseId}")]
    public async Task<IActionResult> UpdateChronicDisease(Guid PatientId, Guid chronicDiseaseId, [FromBody] UpdatePatientChronicDiseaseDto updateDto)
    {
        var command = new UpdateChronicDiseaseCommand(PatientId, chronicDiseaseId, updateDto);
        var result = await _mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Remove a chronic disease from a clinic patient
    /// </summary>
    /// <param name="PatientId">The clinic patient ID</param>
    /// <param name="chronicDiseaseId">The chronic disease ID</param>
    /// <returns>Success or error result</returns>
    [HttpDelete("{chronicDiseaseId}")]
    public async Task<IActionResult> RemoveChronicDisease(Guid PatientId, Guid chronicDiseaseId)
    {
        var command = new RemoveChronicDiseaseCommand(PatientId, chronicDiseaseId);
        var result = await _mediator.Send(command);
        return HandleDeleteResult(result);
    }
}
