using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.MedicalSupplies;

public class GetMedicalSuppliesEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/branches/{clinicBranchId:guid}/medical-supplies", HandleAsync)
            .RequireAuthorization("InventoryManagement")
            .WithName("GetMedicalSupplies")
            .WithSummary("Get medical supplies for a clinic branch with pagination")
            .WithTags("MedicalSupplies")
            .Produces<PaginatedResult<MedicalSupply>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        Guid clinicBranchId,
        int pageNumber = 1,
        int pageSize = 10,
        ApplicationDbContext db = null!,
        CurrentUserService currentUser = null!,
        CancellationToken ct = default)
    {
        // Verify branch exists (global query filter ensures it belongs to clinic)
        var branchExists = await db.ClinicBranches
            .AnyAsync(cb => cb.Id == clinicBranchId, ct);

        if (!branchExists)
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.BRANCH_NOT_FOUND,
                Title = "Branch Not Found",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Branch not found or does not belong to your clinic"
            });

        var query = db.MedicalSupplies
            .Where(ms => ms.ClinicBranchId == clinicBranchId)
            .OrderBy(ms => ms.Name);

        var totalCount = await query.CountAsync(ct);

        var supplies = await query
            .Paginate(pageNumber, pageSize)
            .Select(ms => new MedicalSupply(
                ms.Id,
                ms.Name,
                ms.QuantityInStock,
                ms.UnitPrice,
                ms.IsLowStock
            ))
            .ToListAsync(ct);

        return Results.Ok(new PaginatedResult<MedicalSupply>(
            supplies,
            totalCount,
            pageNumber,
            pageSize
        ));
    }

    public record MedicalSupply(
        Guid Id,
        string Name,
        int QuantityInStock,
        decimal UnitPrice,
        bool IsLowStock);
}
