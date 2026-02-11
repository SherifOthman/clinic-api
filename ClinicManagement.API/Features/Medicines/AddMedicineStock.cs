using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Exceptions;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Medicines;

public class AddMedicineStockEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/medicines/{medicineId:guid}/stock/add", HandleAsync)
            .RequireAuthorization()
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
            medicine.AddStock(request.Strips, request.Reason);

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
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to add medicine stock {MedicineId} Strips={Strips} by {UserId}",
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
        int AddedStrips,
        int NewStock,
        int FullBoxes,
        int RemainingStrips,
        string Message);
}
