using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.MedicalServices.Commands.CreateMedicalService;

public record CreateMedicalServiceCommand : IRequest<Result<Guid>>
{
    public Guid ClinicBranchId { get; init; }
    public string Name { get; init; } = null!;
    public decimal DefaultPrice { get; init; }
    public bool IsOperation { get; init; }
}