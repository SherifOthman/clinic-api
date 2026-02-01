using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.ChronicDiseases.Commands.DeleteChronicDisease;

public class DeleteChronicDiseaseCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; set; }
}
