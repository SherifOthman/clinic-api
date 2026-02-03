using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Medicines.Commands.CreateMedicine;

/// <summary>
/// Command to create a new medicine with comprehensive inventory management
/// </summary>
public record CreateMedicineCommand : IRequest<Result<Guid>>
{
    public Guid ClinicBranchId { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string? Manufacturer { get; init; }
    public string? BatchNumber { get; init; }
    public DateTime? ExpiryDate { get; init; }
    public decimal BoxPrice { get; init; }
    public int StripsPerBox { get; init; }
    public int TotalStripsInStock { get; init; }
    public int MinimumStockLevel { get; init; } = 10;
    public int? ReorderLevel { get; init; }
}
