using ClinicManagement.API.RateLimiting;
using ClinicManagement.Persistence.Seeders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicManagement.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "SuperAdmin")]
[Produces("application/json")]
public class AdminController : ControllerBase
{
    private readonly GeoLocationSeedService _geoSeed;

    public AdminController(GeoLocationSeedService geoSeed) => _geoSeed = geoSeed;

    /// <summary>
    /// Upserts GeoCountries, GeoStates, GeoCities from GeoNames API.
    /// Inserts new rows and updates existing ones — safe to re-run after config changes.
    /// Rows no longer returned by GeoNames are kept (patients may reference them).
    /// Filter behaviour is controlled by GeoNames:CityFilter in appsettings.json.
    /// </summary>
    [HttpPost("geo-seed")]
    [EnableRateLimiting(RateLimitPolicies.UserWrites)]
    [ProducesResponseType(typeof(GeoSeedResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> SeedGeoData(CancellationToken ct)
    {
        var result = await _geoSeed.SeedAsync(ct);
        return Ok(result);
    }
}
