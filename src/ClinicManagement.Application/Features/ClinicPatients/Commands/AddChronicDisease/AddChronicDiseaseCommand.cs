using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.ClinicPatients.Commands.AddChronicDisease;

public record AddChronicDiseaseCommand(
    Guid ClinicPatientId,
    CreateClinicPatientChronicDiseaseDto ChronicDisease
) : IRequest<Result<ClinicPatientChronicDiseaseDto>>;
