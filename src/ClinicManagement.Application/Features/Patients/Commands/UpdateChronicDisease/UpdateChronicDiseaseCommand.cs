using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands.UpdateChronicDisease;

public record UpdateChronicDiseaseCommand(
    Guid PatientId,
    Guid ChronicDiseaseId,
    UpdatePatientChronicDiseaseDto UpdateData
) : IRequest<Result<PatientChronicDiseaseDto>>;
