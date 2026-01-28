using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.ChronicDiseases.Queries.GetChronicDiseases;

public record GetChronicDiseasesQuery : IRequest<Result<List<ChronicDiseaseDto>>>;
