using ClinicManagement.API.Common;

namespace ClinicManagement.API.Features.Locations;

public class GetCountriesEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/locations/countries", HandleAsync)
            .AllowAnonymous()
            .CacheOutput("LocationData")
            .WithName("GetCountries")
            .WithSummary("Get all countries")
            .WithTags("Locations")
            .Produces<List<Response>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> HandleAsync(
        GeoNamesService geoNamesService,
        CancellationToken ct)
    {
        var countries = await geoNamesService.GetCountriesAsync();

        var response = countries
            .Select(c => new Response(
                c.GeonameId, 
                c.Name.En, 
                c.Name.Ar,
                c.CountryCode))
            .OrderBy(c => c.CountryNameEn)
            .ToList();

        return Results.Ok(response);
    }

    public record Response(
        int GeonameId,
        string CountryNameEn,
        string CountryNameAr,
        string CountryCode);
}
