using System.Text.Json;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// GeoNames API client — used only by GeoLocationSeedService at startup.
/// No caching needed: data is seeded into the DB once and never fetched again at runtime.
/// </summary>
public class GeoNamesService : IGeoNamesService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeoNamesService> _logger;
    private readonly GeoNamesOptions _options;
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    public GeoNamesService(
        HttpClient httpClient,
        ILogger<GeoNamesService> logger,
        IOptions<GeoNamesOptions> options)
    {
        _httpClient = httpClient;
        _logger     = logger;
        _options    = options.Value;
    }

    // ── IGeoNamesService ──────────────────────────────────────────────────────

    async Task<List<GeoNamesItem>> IGeoNamesService.GetCountriesAsync(string lang, CancellationToken ct)
    {
        _logger.LogInformation("Fetching countries [{Lang}] from GeoNames", lang);
        var url  = $"{_options.BaseUrl}/countryInfoJSON?username={_options.Username}&lang={lang}";
        var data = await FetchAsync<GeoNamesCountryInfoResponse>(url);
        return (data?.Geonames ?? [])
            .Select(c => new GeoNamesItem(c.GeonameId, c.CountryName, c.CountryCode))
            .OrderBy(c => c.Name)
            .ToList();
    }

    async Task<List<GeoNamesItem>> IGeoNamesService.GetStatesAsync(int countryGeonameId, string lang, CancellationToken ct)
    {
        _logger.LogInformation("Fetching states for country {Id} [{Lang}]", countryGeonameId, lang);
        var url  = $"{_options.BaseUrl}/childrenJSON?geonameId={countryGeonameId}&username={_options.Username}&lang={lang}&maxRows=1000";
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

        var infoUrl   = $"{_options.BaseUrl}/getJSON?geonameId={stateGeonameId}&username={_options.Username}&lang=en";
        var stateInfo = await FetchAsync<GeoNamesLocationInfo>(infoUrl);

        if (stateInfo is null)
        {
            _logger.LogWarning("State {Id} not found in GeoNames", stateGeonameId);
            return [];
        }

        List<GeoNamesChildInfo> cities;

        if (!string.IsNullOrWhiteSpace(stateInfo.AdminCode1) && stateInfo.AdminCode1 != "00")
        {
            var searchUrl  = $"{_options.BaseUrl}/searchJSON?country={stateInfo.CountryCode}&adminCode1={stateInfo.AdminCode1}" +
                             $"&featureClass=P&username={_options.Username}&lang={lang}&maxRows=1000&orderby=population";
            var searchData = await FetchAsync<GeoNamesSearchResponse>(searchUrl);
            cities = (searchData?.Geonames ?? [])
                .Where(g =>
                    g.Fcode == "PPLC"  ||
                    g.Fcode == "PPLA"  ||
                    g.Fcode == "PPLA2" ||
                    g.Fcode == "PPLA3" ||
                    g.Fcode == "PPLA4" ||
                    (g.Fcode == "PPL" && g.Population >= 50_000))
                .Select(g => new GeoNamesChildInfo { GeonameId = g.GeonameId, Name = g.Name, Fcode = g.Fcode })
                .ToList();
        }
        else
        {
            var childUrl  = $"{_options.BaseUrl}/childrenJSON?geonameId={stateGeonameId}&username={_options.Username}&lang={lang}&maxRows=1000";
            var childData = await FetchAsync<GeoNamesResponse<GeoNamesChildInfo>>(childUrl);
            cities = (childData?.Geonames ?? [])
                .Where(c => c.Fcode is "PPLC" or "PPLA" or "PPLA2" or "PPLA3" or "PPLA4")
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
