using ClinicManagement.API.Common;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.ChronicDiseases;

public class GetChronicDiseasesEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/chronic-diseases", HandleAsync)
            .AllowAnonymous()
            .CacheOutput("ReferenceData")
            .WithName("GetChronicDiseases")
            .WithSummary("Get all chronic diseases")
            .WithTags("Chronic Diseases")
            .Produces<List<Response>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        [FromQuery] string? language,
        ApplicationDbContext db,
        CancellationToken ct)
    {
        var isArabic = language?.ToLower() == "ar";

        var diseases = await db.ChronicDiseases
            .OrderBy(cd => isArabic ? cd.NameAr : cd.NameEn)
            .Select(cd => new Response(
                cd.Id,
                isArabic ? cd.NameAr : cd.NameEn
            ))
            .ToListAsync(ct);

        return Results.Ok(diseases);
    }

    public record Response(
        Guid Id,
        string Name);
}
