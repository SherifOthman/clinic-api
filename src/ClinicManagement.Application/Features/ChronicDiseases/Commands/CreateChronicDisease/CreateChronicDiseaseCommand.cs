using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.ChronicDiseases.Commands.CreateChronicDisease;

public class CreateChronicDiseaseCommand : IRequest<Result<ChronicDiseaseDto>>
{
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public bool IsActive { get; set; } = true;
}
