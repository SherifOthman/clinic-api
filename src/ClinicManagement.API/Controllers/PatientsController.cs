using ClinicManagement.Application.Features.Patients.Commands.CreatePatient;
using ClinicManagement.Application.Features.Patients.Commands.UpdatePatient;
using ClinicManagement.Application.Features.Patients.Queries.GetPatientById;
using ClinicManagement.Application.Features.Patients.Queries.GetPatients;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PatientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetPatients([FromQuery] GetPatientsQuery query)
    {
        var result = await _mediator.Send(query);
        
        if (result.Success)
        {
            return Ok(result.Value);
        }
        
        return BadRequest(result.Message);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPatientById(int id)
    {
        var query = new GetPatientByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (result.Success)
        {
            return Ok(result.Value);
        }
        
        return NotFound(result.Message);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePatient(CreatePatientCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (result.Success)
        {
            return CreatedAtAction(nameof(GetPatientById), new { id = result.Value.Id }, result.Value);
        }
        
        return BadRequest(result.Message);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePatient(int id, UpdatePatientCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        
        if (result.Success)
        {
            return Ok(result.Value);
        }
        
        return BadRequest(result.Message);
    }
}
