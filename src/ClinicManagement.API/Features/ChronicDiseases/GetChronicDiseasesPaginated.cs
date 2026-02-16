using ClinicManagement.API.Common;
using ClinicManagement.API.Common.Models;
using ClinicManagement.API.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Features.ChronicDiseases;

public class GetChronicDiseasesPaginatedEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/chronic-diseases/paginated", HandleAsync)
            .AllowAnonymous()
            .CacheOutput("ReferenceData")
            .WithName("GetChronicDiseasesPaginated")
            .WithSummary("Get chronic diseases with pagination")
            .WithTags("Chronic Diseases")
            .Produces<PaginatedResult<ChronicDisease>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        ApplicationDbContext db,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var query = db.ChronicDiseases
            .OrderBy(cd => cd.NameEn);

        var totalCount = await query.CountAsync(ct);

        var diseases = await query
            .Paginate(pageNumber, pageSize)
            .Select(cd => new ChronicDisease(
                cd.Id,
                cd.NameEn
            ))
            .ToListAsync(ct);

        return Results.Ok(new PaginatedResult<ChronicDisease>(
            diseases,
            totalCount,
            pageNumber,
            pageSize
        ));
    }

    public record ChronicDisease(
        Guid Id,
        string Name);
}
