using System.Text.Json;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// GeoNames API client — used only by GeoLocationSeedService at startup.
/// Uses IOptionsSnapshot so config changes in appsettings.json take effect
/// on the next seed request without restarting the application.
/// </summary>
public class GeoNamesService : IGeoNamesService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeoNamesService> _logger;
    private readonly IOptionsSnapshot<GeoNamesOptions> _options;
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public GeoNamesService(
        HttpClient httpClient,
        ILogger<GeoNamesService> logger,
        IOptionsSnapshot<GeoNamesOptions> options)
    {
        _httpClient = httpClient;
        _logger     = logger;
        _options    = options;
    }

    private GeoNamesOptions Options => _options.Value;

    // ── IGeoNamesService ──────────────────────────────────────────────────────

    async Task<List<GeoNamesItem>> IGeoNamesService.GetCountriesAsync(string lang, CancellationToken ct)
    {
        _logger.LogInformation("Fetching countries [{Lang}] from GeoNames", lang);
        var url  = $"{Options.BaseUrl}/countryInfoJSON?username={Options.Username}&lang={lang}";
        var data = await FetchAsync<GeoNamesCountryInfoResponse>(url);
        return (data?.Geonames ?? [])
            .Select(c => new GeoNamesItem(c.GeonameId, c.CountryName, c.CountryCode))
            .OrderBy(c => c.Name)
            .ToList();
    }

    async Task<List<GeoNamesItem>> IGeoNamesService.GetStatesAsync(int countryGeonameId, string lang, CancellationToken ct)
    {
        _logger.LogInformation("Fetching states for country {Id} [{Lang}]", countryGeonameId, lang);
        var url  = $"{Options.BaseUrl}/childrenJSON?geonameId={countryGeonameId}&username={Options.Username}&lang={lang}&maxRows=1000";
        var data = await FetchAsync<GeoNamesResponse<GeoNamesChildInfo>>(url);
        return (data?.Geonames ?? [])
            .Where(s => s.Fcode.StartsWith("ADM1"))
            .Select(s => new GeoNamesItem(s.GeonameId, s.Name))
            .OrderBy(s => s.Name)
            .ToList();
    }

    async Task<List<GeoNamesItem>> IGeoNamesService.GetCitiesAsync(int stateGeonameId, string lang, CancellationToken ct)
    {
        _logger.LogInformation("Fetching cities for state {Id} [{Lang}]", stateGeonameId, lang);

        var infoUrl   = $"{Options.BaseUrl}/getJSON?geonameId={stateGeonameId}&username={Options.Username}&lang=en";
        var stateInfo = await FetchAsync<GeoNamesLocationInfo>(infoUrl);

        if (stateInfo is null)
        {
            _logger.LogWarning("State {Id} not found in GeoNames", stateGeonameId);
            return [];
        }

        var filter   = Options.CityFilter;
        var maxRows  = filter.MaxRows;
        var alwaysFc = filter.AlwaysIncludeFeatureCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var minPop   = filter.MinPopulationForPpl;

        List<GeoNamesChildInfo> cities;

        if (!string.IsNullOrWhiteSpace(stateInfo.AdminCode1) && stateInfo.AdminCode1 != "00")
        {
            var searchUrl  = $"{Options.BaseUrl}/searchJSON?country={stateInfo.CountryCode}&adminCode1={stateInfo.AdminCode1}" +
                             $"&featureClass=P&username={Options.Username}&lang={lang}&maxRows={maxRows}&orderby=population";
            var searchData = await FetchAsync<GeoNamesSearchResponse>(searchUrl);
            cities = (searchData?.Geonames ?? [])
                .Where(g => alwaysFc.Contains(g.Fcode) || (g.Fcode == "PPL" && g.Population >= minPop))
                .Select(g => new GeoNamesChildInfo { GeonameId = g.GeonameId, Name = g.Name, Fcode = g.Fcode })
                .ToList();
        }
        else
        {
            var childUrl  = $"{Options.BaseUrl}/childrenJSON?geonameId={stateGeonameId}&username={Options.Username}&lang={lang}&maxRows={maxRows}";
            var childData = await FetchAsync<GeoNamesResponse<GeoNamesChildInfo>>(childUrl);
            cities = (childData?.Geonames ?? [])
                .Where(c => alwaysFc.Contains(c.Fcode))
                .ToList();
        }

        return cities
            .Select(c => new GeoNamesItem(c.GeonameId, c.Name))
            .OrderBy(c => c.Name)
            .ToList();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<T?> FetchAsync<T>(string url)
    {
        var response = await _httpClient.GetAsync(url, CancellationToken.None);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, _json);
    }

    // ── Response models ───────────────────────────────────────────────────────

    private class GeoNamesCountryInfoResponse { public List<GeoNamesCountryInfo> Geonames { get; set; } = []; }
    private class GeoNamesCountryInfo  { public int GeonameId { get; set; } public string CountryCode { get; set; } = null!; public string CountryName { get; set; } = null!; }
    private class GeoNamesChildInfo    { public int GeonameId { get; set; } public string Name { get; set; } = null!; public string Fcode { get; set; } = null!; }
    private class GeoNamesResponse<T>  { public List<T> Geonames { get; set; } = []; }
    private class GeoNamesLocationInfo { public int GeonameId { get; set; } public string CountryCode { get; set; } = null!; public string AdminCode1 { get; set; } = null!; }
    private class GeoNamesSearchResponse { public List<GeoNamesSearchResult> Geonames { get; set; } = []; }
    private class GeoNamesSearchResult { public int GeonameId { get; set; } public string Name { get; set; } = null!; public string Fcode { get; set; } = null!; public int Population { get; set; } }
}
