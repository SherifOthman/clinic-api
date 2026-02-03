using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.MedicalSupplies.Commands.CreateMedicalSupply;

public record CreateMedicalSupplyCommand : IRequest<Result<Guid>>
{
    public Guid ClinicBranchId { get; init; }
    public string Name { get; init; } = null!;
    public decimal UnitPrice { get; init; }
    public int QuantityInStock { get; init; }
    public int MinimumStockLevel { get; init; } = 10;
}
