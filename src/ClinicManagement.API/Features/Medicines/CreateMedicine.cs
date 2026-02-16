using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;
using ClinicManagement.API.Common.Validation;
using ClinicManagement.API.Common.Enums;
using ClinicManagement.API.Common.Exceptions;
using ClinicManagement.API.Entities;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Medicines;

public class CreateMedicineEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/medicines", HandleAsync)
            .RequireAuthorization("InventoryManagement")
            .WithName("CreateMedicine")
            .WithSummary("Create a new medicine")
            .WithTags("Medicines")
            .Produces<Response>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            ;
    }

    private static async Task<IResult> HandleAsync(
        Request request,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
    {
        // Verify branch exists (global query filter ensures it belongs to clinic)
        var branchExists = await db.ClinicBranches
            .AnyAsync(b => b.Id == request.ClinicBranchId, ct);

        if (!branchExists)
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.BRANCH_NOT_FOUND,
                Title = "Branch Not Found",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Branch not found or does not belong to your clinic"
            });

        try
        {
            var medicine = new Medicine
            {
                ClinicBranchId = request.ClinicBranchId,
                Name = request.Name,
                BoxPrice = request.BoxPrice,
                StripsPerBox = request.StripsPerBox,
                TotalStripsInStock = request.InitialStock,
                Description = request.Description,
                Manufacturer = request.Manufacturer,
                BatchNumber = request.BatchNumber,
                ExpiryDate = request.ExpiryDate,
                MinimumStockLevel = request.MinimumStockLevel,
                ReorderLevel = request.ReorderLevel,
                IsActive = true,
                IsDiscontinued = false
            };

            db.Medicines.Add(medicine);
            await db.SaveChangesAsync(ct);

            // Load response with related data
            var response = await db.Medicines
                .Where(m => m.Id == medicine.Id)
                .Select(m => new Response(
                    m.Id,
                    m.ClinicBranchId,
                    m.Name,
                    m.Description,
                    m.Manufacturer,
                    m.BoxPrice,
                    m.StripPrice,
                    m.StripsPerBox,
                    m.TotalStripsInStock,
                    m.StockStatus,
                    m.IsActive,
                    m.CreatedAt
                ))
                .FirstAsync(ct);

            return Results.Created($"/api/medicines/{response.Id}", response);
        }
        catch (Exception ex)
        {
            return ex.HandleDomainException();
        }
    }

    public record Request(
        [Required]
        Guid ClinicBranchId,
        
        [Required]
        [MaxLength(100)]
        string Name,
        
        [Required]
        [Range(0.01, double.MaxValue)]
        decimal BoxPrice,
        
        [Required]
        [Range(1, int.MaxValue)]
        int StripsPerBox,
        
        [Range(0, int.MaxValue)]
        int InitialStock = 0,
        
        string? Description = null,
        string? Manufacturer = null,
        string? BatchNumber = null,
        
        [CustomValidation(typeof(CustomValidators), nameof(CustomValidators.MustBeInFutureOrNull))]
        DateTime? ExpiryDate = null,
        
        [Range(0, int.MaxValue)]
        int MinimumStockLevel = 10,
        
        [Range(0, int.MaxValue)]
        int ReorderLevel = 20);

    public record Response(
        Guid Id,
        Guid ClinicBranchId,
        string Name,
        string? Description,
        string? Manufacturer,
        decimal BoxPrice,
        decimal StripPrice,
        int StripsPerBox,
        int TotalStripsInStock,
        StockStatus StockStatus,
        bool IsActive,
        DateTime CreatedAt);
}
