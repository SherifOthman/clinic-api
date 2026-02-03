using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.ClinicPatients.Queries.GetChronicDiseases;

public record GetChronicDiseasesQuery(
    Guid ClinicPatientId,
    bool ActiveOnly = false
) : IRequest<Result<IEnumerable<ClinicPatientChronicDiseaseDto>>>;