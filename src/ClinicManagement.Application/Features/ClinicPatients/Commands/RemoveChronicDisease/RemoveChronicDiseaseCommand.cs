using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.ClinicPatients.Commands.RemoveChronicDisease;

public record RemoveChronicDiseaseCommand(
    Guid ClinicPatientId,
    Guid ChronicDiseaseId
) : IRequest<Result>;
