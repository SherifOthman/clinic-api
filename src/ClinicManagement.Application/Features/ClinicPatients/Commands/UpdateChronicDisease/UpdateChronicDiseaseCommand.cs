using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.ClinicPatients.Commands.UpdateChronicDisease;

public record UpdateChronicDiseaseCommand(
    Guid ClinicPatientId,
    Guid ChronicDiseaseId,
    UpdateClinicPatientChronicDiseaseDto UpdateData
) : IRequest<Result<ClinicPatientChronicDiseaseDto>>;
