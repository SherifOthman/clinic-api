using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Interfaces;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpecializationsController : BaseApiController
{
    private readonly ISpecializationRepository _specializationRepository;

    public SpecializationsController(IMediator mediator, ISpecializationRepository specializationRepository) 
        : base(mediator)
    {
        _specializationRepository = specializationRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SpecializationDto>>> GetSpecializations()
    {
        var specializations = await _specializationRepository.GetActiveSpecializationsAsync();
        var specializationDtos = specializations.Adapt<IEnumerable<SpecializationDto>>();
        return Ok(specializationDtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SpecializationDto>> GetSpecialization(Guid id)
    {
        var specialization = await _specializationRepository.GetByIdAsync(id);
        if (specialization == null)
        {
            return NotFound();
        }

        var specializationDto = specialization.Adapt<SpecializationDto>();
        return Ok(specializationDto);
    }
}