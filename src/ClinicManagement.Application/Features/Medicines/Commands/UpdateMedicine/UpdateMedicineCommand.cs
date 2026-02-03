using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Medicines.Commands.UpdateMedicine;

public record UpdateMedicineCommand : IRequest<Result>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public decimal BoxPrice { get; init; }
    public int StripsPerBox { get; init; }
    public int TotalStripsInStock { get; init; }
    public int MinimumStockLevel { get; init; }
}
