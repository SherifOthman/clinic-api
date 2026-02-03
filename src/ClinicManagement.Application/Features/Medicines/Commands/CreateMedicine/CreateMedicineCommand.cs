using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Medicines.Commands.CreateMedicine;

public record CreateMedicineCommand : IRequest<Result<Guid>>
{
    public Guid ClinicBranchId { get; init; }
    public string Name { get; init; } = null!;
    public decimal BoxPrice { get; init; }
    public int StripsPerBox { get; init; }
    public int TotalStripsInStock { get; init; }
    public int MinimumStockLevel { get; init; } = 10;
}