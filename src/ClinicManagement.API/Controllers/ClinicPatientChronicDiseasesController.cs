using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.ClinicPatients.Commands.AddChronicDisease;
using ClinicManagement.Application.Features.ClinicPatients.Commands.RemoveChronicDisease;
using ClinicManagement.Application.Features.ClinicPatients.Commands.UpdateChronicDisease;
using ClinicManagement.Application.Features.ClinicPatients.Queries.GetChronicDiseases;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/clinic-patients/{clinicPatientId}/chronic-diseases")]
public class ClinicPatientChronicDiseasesController : BaseApiController
{
    public ClinicPatientChronicDiseasesController(IMediator mediator) : base(mediator)
    {
    }

    /// <summary>
    /// Get all chronic diseases for a clinic patient
    /// </summary>
    /// <param name="clinicPatientId">The clinic patient ID</param>
    /// <param name="activeOnly">Whether to return only active chronic diseases</param>
    /// <returns>List of chronic diseases</returns>
    [HttpGet]
    public async Task<IActionResult> GetChronicDiseases(Guid clinicPatientId, [FromQuery] bool activeOnly = false)
    {
        var query = new GetChronicDiseasesQuery(clinicPatientId, activeOnly);
        var result = await Mediator.Send(query);
        return HandleResult(result);
    }

    /// <summary>
    /// Add a chronic disease to a clinic patient
    /// </summary>
    /// <param name="clinicPatientId">The clinic patient ID</param>
    /// <param name="createDto">The chronic disease data</param>
    /// <returns>The created chronic disease relationship</returns>
    [HttpPost]
    public async Task<IActionResult> AddChronicDisease(Guid clinicPatientId, [FromBody] CreateClinicPatientChronicDiseaseDto createDto)
    {
        var command = new AddChronicDiseaseCommand(clinicPatientId, createDto);
        var result = await Mediator.Send(command);
        return HandleCreateResult(result, nameof(GetChronicDiseases), new { clinicPatientId });
    }

    /// <summary>
    /// Update a chronic disease relationship for a clinic patient
    /// </summary>
    /// <param name="clinicPatientId">The clinic patient ID</param>
    /// <param name="chronicDiseaseId">The chronic disease ID</param>
    /// <param name="updateDto">The update data</param>
    /// <returns>The updated chronic disease relationship</returns>
    [HttpPut("{chronicDiseaseId}")]
    public async Task<IActionResult> UpdateChronicDisease(Guid clinicPatientId, Guid chronicDiseaseId, [FromBody] UpdateClinicPatientChronicDiseaseDto updateDto)
    {
        var command = new UpdateChronicDiseaseCommand(clinicPatientId, chronicDiseaseId, updateDto);
        var result = await Mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Remove a chronic disease from a clinic patient
    /// </summary>
    /// <param name="clinicPatientId">The clinic patient ID</param>
    /// <param name="chronicDiseaseId">The chronic disease ID</param>
    /// <returns>Success or error result</returns>
    [HttpDelete("{chronicDiseaseId}")]
    public async Task<IActionResult> RemoveChronicDisease(Guid clinicPatientId, Guid chronicDiseaseId)
    {
        var command = new RemoveChronicDiseaseCommand(clinicPatientId, chronicDiseaseId);
        var result = await Mediator.Send(command);
        return HandleDeleteResult(result);
    }
}