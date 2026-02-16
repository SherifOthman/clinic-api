using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Extensions;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Medicines;

public class RemoveMedicineStockEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/medicines/{medicineId:guid}/stock/remove", HandleAsync)
            .RequireAuthorization("InventoryManagement")
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
        HttpContext httpContext,
        CancellationToken ct)
    {
        // Load medicine - ClinicId filter is automatic via global query filter
        var medicine = await db.Medicines
            .Where(m => m.Id == medicineId)
            .FirstOrDefaultAsync(ct);

        if (medicine is null)
            return Results.NotFound(new { error = "Medicine not found", code = "NOT_FOUND" });

        var previousStock = medicine.TotalStripsInStock;

        // Business logic validation
        if (request.Strips > medicine.TotalStripsInStock)
        {
            var modelState = new ModelStateDictionary();
            modelState.AddGeneralError("INSUFFICIENT_STOCK");
            
            logger.LogWarning(
                "Insufficient stock for medicine {MedicineId}: Requested={Requested}, Available={Available}",
                medicineId, request.Strips, medicine.TotalStripsInStock);
            
            return modelState.ToValidationProblem("INSUFFICIENT_STOCK", httpContext);
        }

        // Update stock
        medicine.TotalStripsInStock -= request.Strips;

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

    public record Request(
        [Required]
        [Range(1, int.MaxValue)]
        int Strips,
        
        [MaxLength(255)]
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
