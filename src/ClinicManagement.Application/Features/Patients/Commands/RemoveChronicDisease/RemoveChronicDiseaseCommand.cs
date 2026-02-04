using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Commands.RemoveChronicDisease;

public record RemoveChronicDiseaseCommand(
    Guid PatientId,
    Guid ChronicDiseaseId
) : IRequest<Result>;
