using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Medicines;

public class GetMedicineByIdEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/medicines/{id:guid}", HandleAsync)
            .RequireAuthorization("InventoryManagement")
            .WithName("GetMedicineById")
            .WithSummary("Get medicine details by ID")
            .WithTags("Medicines")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
    {
        // ClinicId filter is automatic via global query filter
        var medicine = await db.Medicines
            .Where(m => m.Id == id)
            .Select(m => new Response(
                m.Id,
                m.ClinicBranchId,
                m.ClinicBranch.Name,
                m.Name,
                m.Description,
                m.Manufacturer,
                m.BatchNumber,
                m.ExpiryDate,
                m.DaysUntilExpiry,
                m.BoxPrice,
                m.StripPrice,
                m.StripsPerBox,
                m.TotalStripsInStock,
                m.FullBoxesInStock,
                m.RemainingStrips,
                m.MinimumStockLevel,
                m.ReorderLevel,
                m.StockStatus,
                m.IsActive,
                m.IsDiscontinued,
                m.IsExpired,
                m.IsExpiringSoon,
                m.IsLowStock,
                m.NeedsReorder,
                m.InventoryValue,
                m.CreatedAt,
                m.UpdatedAt
            ))
            .FirstOrDefaultAsync(ct);

        return medicine is null
            ? Results.NotFound(new { error = "Medicine not found", code = "NOT_FOUND" })
            : Results.Ok(medicine);
    }

    public record Response(
        Guid Id,
        Guid ClinicBranchId,
        string BranchName,
        string Name,
        string? Description,
        string? Manufacturer,
        string? BatchNumber,
        DateTime? ExpiryDate,
        int? DaysUntilExpiry,
        decimal BoxPrice,
        decimal StripPrice,
        int StripsPerBox,
        int TotalStripsInStock,
        int FullBoxesInStock,
        int RemainingStrips,
        int MinimumStockLevel,
        int ReorderLevel,
        StockStatus StockStatus,
        bool IsActive,
        bool IsDiscontinued,
        bool IsExpired,
        bool IsExpiringSoon,
        bool IsLowStock,
        bool NeedsReorder,
        decimal InventoryValue,
        DateTime CreatedAt,
        DateTime? UpdatedAt);
}
