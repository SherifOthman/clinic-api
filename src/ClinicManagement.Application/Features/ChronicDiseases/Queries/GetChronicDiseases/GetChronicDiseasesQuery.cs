using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.ChronicDiseases.Queries.GetChronicDiseases;

public record GetChronicDiseasesQuery(string? Language = null) : IRequest<Result<List<ChronicDiseaseDto>>>;
