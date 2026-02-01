using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.ChronicDiseases.Commands.UpdateChronicDisease;

public class UpdateChronicDiseaseCommand : IRequest<Result<ChronicDiseaseDto>>
{
    public int Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public bool IsActive { get; set; }
}
