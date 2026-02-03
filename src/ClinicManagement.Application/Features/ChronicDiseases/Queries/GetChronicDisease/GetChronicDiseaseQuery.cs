using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.ChronicDiseases.Queries.GetChronicDisease;

public class GetChronicDiseaseQuery : IRequest<Result<ChronicDiseaseDto>>
{
    public Guid Id { get; set; }
}
