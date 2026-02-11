using ClinicManagement.API.Common;
using ClinicManagement.API.Entities;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.MedicalSupplies;

public class CreateMedicalSupplyEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/branches/{clinicBranchId:guid}/medical-supplies", HandleAsync)
            .RequireAuthorization()
            .WithName("CreateMedicalSupply")
            .WithSummary("Create a new medical supply")
            .WithTags("MedicalSupplies")
            .Produces<Response>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            ;
    }

    private static async Task<IResult> HandleAsync(
        Guid clinicBranchId,
        Request request,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
    {
        // Verify branch exists (global query filter ensures it belongs to clinic)
        var branchExists = await db.ClinicBranches
            .AnyAsync(cb => cb.Id == clinicBranchId, ct);

        if (!branchExists)
            return Results.BadRequest(new
            {
                error = "Branch not found or does not belong to your clinic",
                code = "BRANCH_NOT_FOUND"
            });

        // Check for duplicate name
        var duplicateExists = await db.MedicalSupplies
            .AnyAsync(ms =>
                ms.ClinicBranchId == clinicBranchId &&
                ms.Name == request.Name, ct);

        if (duplicateExists)
            return Results.BadRequest(new
            {
                error = "A medical supply with this name already exists in this branch",
                code = "DUPLICATE_SUPPLY"
            });

        var supply = new MedicalSupply
        {
            Id = Guid.NewGuid(),
            ClinicBranchId = clinicBranchId,
            Name = request.Name,
            QuantityInStock = request.QuantityInStock,
            UnitPrice = request.UnitPrice,
            MinimumStockLevel = request.MinimumStockLevel,
            CreatedAt = DateTime.UtcNow
        };

        db.MedicalSupplies.Add(supply);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/medical-supplies/{supply.Id}", new Response(supply.Id));
    }

    public record Request(
        [Required]
        [MaxLength(200, ErrorMessage = "Supply name must not exceed 200 characters")]
        string Name,
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity in stock cannot be negative")]
        int QuantityInStock,
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Unit price cannot be negative")]
        decimal UnitPrice,
        
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Minimum stock level cannot be negative")]
        int MinimumStockLevel);

    public record Response(Guid Id);
}
