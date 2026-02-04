using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries.GetChronicDiseases;

public record GetChronicDiseasesQuery(
    Guid PatientId,
    bool ActiveOnly = false
) : IRequest<Result<IEnumerable<PatientChronicDiseaseDto>>>;
