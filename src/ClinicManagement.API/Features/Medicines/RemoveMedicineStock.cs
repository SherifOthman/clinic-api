using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Exceptions;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Medicines;

public class RemoveMedicineStockEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/medicines/{medicineId:guid}/stock/remove", HandleAsync)
            .RequireAuthorization()
            .WithName("RemoveMedicineStock")
            .WithSummary("Remove stock from a medicine")
            .WithTags("Medicines")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            ;
    }

    private static async Task<IResult> HandleAsync(
        Guid medicineId,
        Request request,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        ILogger<RemoveMedicineStockEndpoint> logger,
        CancellationToken ct)
    {
        // Load medicine - ClinicId filter is automatic via global query filter
        var medicine = await db.Medicines
            .Where(m => m.Id == medicineId)
            .FirstOrDefaultAsync(ct);

        if (medicine is null)
            return Results.NotFound(new { error = "Medicine not found", code = "NOT_FOUND" });

        var previousStock = medicine.TotalStripsInStock;

        try
        {
            // Use domain method
            medicine.RemoveStock(request.Strips, request.Reason);

            await db.SaveChangesAsync(ct);

            logger.LogWarning(
                "Medicine stock removed: {MedicineId} {MedicineName} PreviousStock={PreviousStock} Removed={RemovedStrips} NewStock={NewStock} Reason={Reason} by {UserId}",
                medicineId, medicine.Name, previousStock, request.Strips, medicine.TotalStripsInStock, request.Reason, currentUser.UserId);

            var response = new Response(
                medicine.Id,
                medicine.Name,
                previousStock,
                -request.Strips, // Negative to indicate removal
                medicine.TotalStripsInStock,
                medicine.FullBoxesInStock,
                medicine.RemainingStrips,
                $"Successfully removed {request.Strips} strips from {medicine.Name}");

            return Results.Ok(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to remove medicine stock {MedicineId} Strips={Strips} by {UserId}",
                medicineId, request.Strips, currentUser.UserId);
            return ex.HandleDomainException();
        }
    }

    public record Request(
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Strips must be greater than 0")]
        int Strips,
        
        [MaxLength(500, ErrorMessage = "Reason must not exceed 500 characters")]
        string? Reason = null);

    public record Response(
        Guid MedicineId,
        string MedicineName,
        int PreviousStock,
        int RemovedStrips,
        int NewStock,
        int FullBoxes,
        int RemainingStrips,
        string Message);
}
