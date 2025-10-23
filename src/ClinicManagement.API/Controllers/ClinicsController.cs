using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Features.Clinics.Commands.CreateClinic;
using ClinicManagement.Application.Features.Clinics.Commands.UpdateClinic;
using ClinicManagement.Application.Features.Clinics.Queries.GetClinicById;
using ClinicManagement.Application.Features.Clinics.Queries.GetClinics;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClinicsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClinicsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetClinics([FromQuery] GetClinicsQuery query)
    {
        var result = await _mediator.Send(query);
        
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        
        return BadRequest(result.Error);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClinicById(int id)
    {
        var query = new GetClinicByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        
        return NotFound(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> CreateClinic(CreateClinicCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetClinicById), new { id = result.Value.Id }, result.Value);
        }
        
        return BadRequest(result.Error);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateClinic(int id, UpdateClinicCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        
        return BadRequest(result.Error);
    }
}
