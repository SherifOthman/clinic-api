using ClinicManagement.API.Common;
using ClinicManagement.API.Entities;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.MedicalServices;

public class CreateMedicalServiceEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/branches/{clinicBranchId:guid}/medical-services", HandleAsync)
            .RequireAuthorization()
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
        var clinicId = currentUser.ClinicId!.Value;

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
        var duplicateExists = await db.MedicalServices
            .AnyAsync(ms =>
                ms.ClinicBranchId == clinicBranchId &&
                ms.Name == request.Name, ct);

        if (duplicateExists)
            return Results.BadRequest(new
            {
                error = "A medical service with this name already exists in this branch",
                code = "DUPLICATE_SERVICE"
            });

        var service = new MedicalService
        {
            Id = Guid.NewGuid(),
            ClinicBranchId = clinicBranchId,
            Name = request.Name,
            DefaultPrice = request.DefaultPrice,
            IsOperation = request.IsOperation,
            CreatedAt = DateTime.UtcNow
        };

        db.MedicalServices.Add(service);
        await db.SaveChangesAsync(ct);

        return Results.Created($"/api/medical-services/{service.Id}", new Response(service.Id));
    }

    public record Request(
        [Required]
        [MaxLength(200, ErrorMessage = "Service name must not exceed 200 characters")]
        string Name,
        
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Default price cannot be negative")]
        decimal DefaultPrice,
        
        bool IsOperation);

    public record Response(Guid Id);
}
