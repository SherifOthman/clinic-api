using ClinicManagement.API.Common;
using Microsoft.AspNetCore.Mvc;

namespace ClinicManagement.API.Features.Locations;

public class GetStatesEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/locations/states", HandleAsync)
            .AllowAnonymous()
            .WithName("GetStates")
            .WithSummary("Get states by country")
            .WithTags("Locations")
            .Produces<List<Response>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        [FromQuery] int countryGeonameId,
        GeoNamesService geoNamesService,
        CancellationToken ct)
    {
        var states = await geoNamesService.GetStatesAsync(countryGeonameId);

        var response = states
            .Select(s => new Response(s.GeonameId, s.Name.En))
            .OrderBy(s => s.Name)
            .ToList();

        return Results.Ok(response);
    }

    public record Response(
        int GeonameId,
        string Name);
}
