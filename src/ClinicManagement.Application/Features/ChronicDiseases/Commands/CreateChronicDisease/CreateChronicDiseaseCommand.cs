using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.ChronicDiseases.Commands.CreateChronicDisease;

public class CreateChronicDiseaseCommand : IRequest<Result<ChronicDiseaseDto>>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
