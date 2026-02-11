using ClinicManagement.API.Common;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Specializations;

public class GetSpecializationByIdEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/specializations/{id:guid}", HandleAsync)
            .WithName("GetSpecializationById")
            .WithSummary("Get a specialization by ID")
            .WithTags("Specializations")
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> HandleAsync(
        Guid id,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var specialization = await db.Specializations
            .Where(s => s.Id == id)
            .Select(s => new Response(
                s.Id,
                s.NameEn,
                s.NameAr
            ))
            .FirstOrDefaultAsync(ct);

        return specialization is null
            ? Results.NotFound(new { error = "Specialization not found", code = "NOT_FOUND" })
            : Results.Ok(specialization);
    }

    public record Response(
        Guid Id,
        string NameEn,
        string NameAr);
}
