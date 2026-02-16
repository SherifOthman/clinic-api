using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.MedicalServices;

public class GetMedicalServicesEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/branches/{clinicBranchId:guid}/medical-services", HandleAsync)
            .RequireAuthorization("InventoryManagement")
            .WithName("GetMedicalServices")
            .WithSummary("Get medical services for a clinic branch")
            .WithTags("MedicalServices")
            .Produces<List<Response>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> HandleAsync(
        Guid clinicBranchId,
        ApplicationDbContext db,
        CurrentUserService currentUser,
        CancellationToken ct)
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

        var services = await db.MedicalServices
            .Where(ms => ms.ClinicBranchId == clinicBranchId)
            .OrderBy(ms => ms.Name)
            .Select(ms => new Response(
                ms.Id,
                ms.Name,
                ms.DefaultPrice,
                ms.IsOperation
            ))
            .ToListAsync(ct);

        return Results.Ok(services);
    }

    public record Response(
        Guid Id,
        string Name,
        decimal DefaultPrice,
        bool IsOperation);
}
