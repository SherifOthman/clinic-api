using ClinicManagement.API.Common;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.Specializations;

public class GetSpecializationsEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/specializations", HandleAsync)
            .RequireAuthorization()
            .CacheOutput("ReferenceData")
            .WithName("GetSpecializations")
            .WithSummary("Get all specializations")
            .WithTags("Specializations")
            .Produces<List<Response>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var specializations = await db.Specializations
            .OrderBy(s => s.NameEn)
            .Select(s => new Response(
                s.Id,
                s.NameEn,
                s.NameAr
            ))
            .ToListAsync(ct);

        return Results.Ok(specializations);
    }

    public record Response(
        Guid Id,
        string NameEn,
        string NameAr);
}
