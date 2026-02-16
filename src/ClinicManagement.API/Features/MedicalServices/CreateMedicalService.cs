using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Constants;
using ClinicManagement.API.Common.Models;
using ClinicManagement.API.Entities;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.MedicalServices;

public class CreateMedicalServiceEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/branches/{clinicBranchId:guid}/medical-services", HandleAsync)
            .RequireAuthorization("InventoryManagement")
            .WithName("CreateMedicalService")
            .WithSummary("Create a new medical service")
            .WithTags("MedicalServices")
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
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.BRANCH_NOT_FOUND,
                Title = "Branch Not Found",
                Status = StatusCodes.Status400BadRequest,
                Detail = "Branch not found or does not belong to your clinic"
            });

        // Check for duplicate name
        var duplicateExists = await db.MedicalServices
            .AnyAsync(ms =>
                ms.ClinicBranchId == clinicBranchId &&
                ms.Name == request.Name, ct);

        if (duplicateExists)
            return Results.BadRequest(new ApiProblemDetails
            {
                Code = ErrorCodes.DUPLICATE_SERVICE,
                Title = "Duplicate Service",
                Status = StatusCodes.Status400BadRequest,
                Detail = "A medical service with this name already exists in this branch"
            });

        var service = new MedicalService
        {
            Id = Guid.NewGuid(),
            ClinicBranchId = clinicBranchId,
            Name = request.Name,
            DefaultPrice = request.DefaultPrice,
            IsOperation = request.IsOperation
        };

        db.MedicalServices.Add(service);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/medical-services/{service.Id}", new Response(service.Id));
    }

    public record Request(
        [Required]
        [MaxLength(100)]
        string Name,
        
        [Required]
        [Range(0, double.MaxValue)]
        decimal DefaultPrice,
        
        bool IsOperation);

    public record Response(Guid Id);
}
