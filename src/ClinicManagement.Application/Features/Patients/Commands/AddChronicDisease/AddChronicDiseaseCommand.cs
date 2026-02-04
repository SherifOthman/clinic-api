using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands.AddChronicDisease;

public record AddChronicDiseaseCommand(
    Guid PatientId,
    CreatePatientChronicDiseaseDto ChronicDisease
) : IRequest<Result<PatientChronicDiseaseDto>>;
