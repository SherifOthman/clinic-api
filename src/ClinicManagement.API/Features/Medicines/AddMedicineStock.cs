using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Extensions;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Medicines;

public class AddMedicineStockEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/medicines/{medicineId:guid}/stock/add", HandleAsync)
            .RequireAuthorization("InventoryManagement")
            .WithName("AddMedicineStock")
            .WithSummary("Add stock to a medicine")
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
        ILogger<AddMedicineStockEndpoint> logger,
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
        if (medicine.IsDiscontinued)
        {
            var modelState = new ModelStateDictionary();
            modelState.AddGeneralError("MEDICINE_DISCONTINUED");
            
            logger.LogWarning(
                "Attempted to add stock to discontinued medicine {MedicineId}",
                medicineId);
            
            return modelState.ToValidationProblem("MEDICINE_DISCONTINUED", httpContext);
        }

        // Update stock
        medicine.TotalStripsInStock += request.Strips;

        await db.SaveChangesAsync(ct);

        logger.LogInformation(
            "Medicine stock added: {MedicineId} {MedicineName} PreviousStock={PreviousStock} Added={AddedStrips} NewStock={NewStock} Reason={Reason} by {UserId}",
            medicineId, medicine.Name, previousStock, request.Strips, medicine.TotalStripsInStock, request.Reason, currentUser.UserId);

        var response = new Response(
            medicine.Id,
            medicine.Name,
            previousStock,
            request.Strips,
            medicine.TotalStripsInStock,
            medicine.FullBoxesInStock,
            medicine.RemainingStrips,
            $"Successfully added {request.Strips} strips to {medicine.Name}");

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
        int AddedStrips,
        int NewStock,
        int FullBoxes,
        int RemainingStrips,
        string Message);
}
