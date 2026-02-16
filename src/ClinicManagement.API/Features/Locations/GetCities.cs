using ClinicManagement.API.Common;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Features.Locations;

public class GetCitiesEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/locations/states/{stateGeonameId}/cities", HandleAsync)
            .AllowAnonymous()
            .CacheOutput("LocationData")
            .WithName("GetCities")
            .WithSummary("Get cities by state")
            .WithTags("Locations")
            .Produces<List<Response>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        int stateGeonameId,
        GeoNamesService geoNamesService,
        CancellationToken ct)
    {
        var cities = await geoNamesService.GetCitiesAsync(stateGeonameId, ct);

        var response = cities
            .Select(c => new Response(
                c.GeonameId, 
                c.Name.En,
                c.Name.Ar))
            .OrderBy(c => c.NameEn)
            .ToList();

        return Results.Ok(response);
    }

    public record Response(
        int GeonameId,
        string NameEn,
        string NameAr);
}
