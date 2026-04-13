using System.Text.Json;
using ClinicManagement.Infrastructure.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// GeoNames API proxy — single language per request, cached by (resource + language).
///
/// The frontend sends the current language ("en" or "ar") as a query param.
/// We call GeoNames once per request (not twice for bilingual merge).
/// Cache key includes the language so EN and AR are cached independently.
/// Cache duration: 24h — location data is static.
/// </summary>
public class GeoNamesService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeoNamesService> _logger;
    private readonly IMemoryCache _cache;
    private readonly GeoNamesOptions _options;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    private static readonly JsonSerializerOptions _json = new() { PropertyNameCaseInsensitive = true };

    // Per-key semaphore prevents duplicate in-flight requests
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

    public GeoNamesService(
        HttpClient httpClient,
        ILogger<GeoNamesService> logger,
        IMemoryCache cache,
        IOptions<GeoNamesOptions> options)
    {
        _httpClient = httpClient;
        _logger     = logger;
        _cache      = cache;
        _options    = options.Value;
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public Task<List<GeoNamesCountry>> GetCountriesAsync(string lang = "en", CancellationToken ct = default)
        => GetOrFetchAsync($"countries_{lang}", async () =>
        {
            _logger.LogInformation("Fetching countries [{Lang}] from GeoNames (cache miss)", lang);
            var url  = $"{_options.BaseUrl}/countryInfoJSON?username={_options.Username}&lang={lang}";
            var data = await FetchAsync<GeoNamesCountryInfoResponse>(url);
            var result = (data?.Geonames ?? [])
                .Select(c => new GeoNamesCountry { GeonameId = c.GeonameId, CountryCode = c.CountryCode, Name = c.CountryName })
                .OrderBy(c => c.Name)
                .ToList();
            _logger.LogInformation("Fetched {Count} countries [{Lang}] — cached 24h", result.Count, lang);
            return result;
        });

    public Task<List<GeoNamesLocation>> GetStatesAsync(int countryGeonameId, string lang = "en", CancellationToken ct = default)
        => GetOrFetchAsync($"states_{countryGeonameId}_{lang}", async () =>
        {
            _logger.LogInformation("Fetching states for country {Id} [{Lang}] (cache miss)", countryGeonameId, lang);
            var url  = $"{_options.BaseUrl}/childrenJSON?geonameId={countryGeonameId}&username={_options.Username}&lang={lang}&maxRows=1000";
            var data = await FetchAsync<GeoNamesResponse<GeoNamesChildInfo>>(url);
            var result = (data?.Geonames ?? [])
                .Where(s => s.Fcode.StartsWith("ADM1"))
                .Select(s => new GeoNamesLocation { GeonameId = s.GeonameId, Fcode = s.Fcode, Name = s.Name })
                .OrderBy(s => s.Name)
                .ToList();
            _logger.LogInformation("Fetched {Count} states for country {Id} [{Lang}] — cached 24h", result.Count, countryGeonameId, lang);
            return result;
        });

    public Task<List<GeoNamesLocation>> GetCitiesAsync(int stateGeonameId, string lang = "en", CancellationToken ct = default)
        => GetOrFetchAsync($"cities_{stateGeonameId}_{lang}", async () =>
        {
            _logger.LogInformation("Fetching cities for state {Id} [{Lang}] (cache miss)", stateGeonameId, lang);

            // Get state metadata to build the search query
            var infoUrl  = $"{_options.BaseUrl}/getJSON?geonameId={stateGeonameId}&username={_options.Username}&lang=en";
            var stateInfo = await FetchAsync<GeoNamesLocationInfo>(infoUrl);

            if (stateInfo is null)
            {
                _logger.LogWarning("State {Id} not found in GeoNames", stateGeonameId);
                return [];
            }

            List<GeoNamesChildInfo> cities;

            // Try search by adminCode1 first (faster, more accurate)
            if (!string.IsNullOrWhiteSpace(stateInfo.AdminCode1) && stateInfo.AdminCode1 != "00")
            {
                var searchUrl = $"{_options.BaseUrl}/searchJSON?country={stateInfo.CountryCode}&adminCode1={stateInfo.AdminCode1}" +
                                $"&featureClass=P&username={_options.Username}&lang={lang}&maxRows=1000&orderby=population";
                var searchData = await FetchAsync<GeoNamesSearchResponse>(searchUrl);
                cities = (searchData?.Geonames ?? [])
                    .Where(g =>
                        g.Fcode == "PPLC"  ||   // capital city
                        g.Fcode == "PPLA"  ||   // seat of 1st-order admin (governorate capital)
                        g.Fcode == "PPLA2" ||   // seat of 2nd-order admin (markaz capital)
                        g.Fcode == "PPLA3" ||   // seat of 3rd-order admin (district capital — e.g. البرج, بلطيم)
                        g.Fcode == "PPLA4" ||   // seat of 4th-order admin
                        (g.Fcode == "PPL" && g.Population >= 50_000))  // other populated places
                    .Select(g => new GeoNamesChildInfo { GeonameId = g.GeonameId, Name = g.Name, Fcode = g.Fcode })
                    .ToList();
            }
            else
            {
                // Fallback: direct children of the state node
                var childUrl  = $"{_options.BaseUrl}/childrenJSON?geonameId={stateGeonameId}&username={_options.Username}&lang={lang}&maxRows=1000";
                var childData = await FetchAsync<GeoNamesResponse<GeoNamesChildInfo>>(childUrl);
                cities = (childData?.Geonames ?? [])
                    .Where(c =>
                        c.Fcode == "PPLC"  ||
                        c.Fcode == "PPLA"  ||
                        c.Fcode == "PPLA2" ||
                        c.Fcode == "PPLA3" ||
                        c.Fcode == "PPLA4")
                    .ToList();
            }

            var result = cities
                .Select(c => new GeoNamesLocation { GeonameId = c.GeonameId, Fcode = c.Fcode, Name = c.Name })
                .OrderBy(c => c.Name)
                .ToList();

            _logger.LogInformation("Fetched {Count} cities for state {Id} [{Lang}] — cached 24h", result.Count, stateGeonameId, lang);
            return result;
        });

    // ── Resolve IDs → names (for filter dropdowns) ───────────────────────────

    /// <summary>
    /// Resolves a set of country GeoNames IDs to { id, name } pairs.
    /// Uses the cached countries list — no extra GeoNames calls if already cached.
    /// </summary>
    public async Task<List<GeoNamesNamedItem>> ResolveCountryNamesAsync(
        IEnumerable<int> ids, string lang = "en", CancellationToken ct = default)
    {
        var idSet = ids.ToHashSet();
        if (idSet.Count == 0) return [];
        var countries = await GetCountriesAsync(lang, ct);
        return countries
            .Where(c => idSet.Contains(c.GeonameId))
            .Select(c => new GeoNamesNamedItem(c.GeonameId, c.Name))
            .ToList();
    }

    /// <summary>
    /// Resolves a set of state GeoNames IDs to { id, name } pairs.
    /// Fetches states for each of the given parent country IDs (all cached after first call).
    /// </summary>
    public async Task<List<GeoNamesNamedItem>> ResolveStateNamesAsync(
        IEnumerable<int> stateIds, IEnumerable<int> parentCountryIds,
        string lang = "en", CancellationToken ct = default)
    {
        var idSet = stateIds.ToHashSet();
        if (idSet.Count == 0) return [];

        var tasks = parentCountryIds.Select(cId => GetStatesAsync(cId, lang, ct));
        var results = await Task.WhenAll(tasks);

        return results
            .SelectMany(list => list)
            .Where(s => idSet.Contains(s.GeonameId))
            .Select(s => new GeoNamesNamedItem(s.GeonameId, s.Name))
            .ToList();
    }

    /// <summary>
    /// Resolves a set of city GeoNames IDs to { id, name } pairs.
    /// Fetches cities for each of the given parent state IDs (cached after first call).
    /// Any IDs not found in the state city lists are resolved individually via getJSON (also cached).
    /// </summary>
    public async Task<List<GeoNamesNamedItem>> ResolveCityNamesAsync(
        IEnumerable<int> cityIds, IEnumerable<int> parentStateIds,
        string lang = "en", CancellationToken ct = default)
    {
        var idSet = cityIds.ToHashSet();
        if (idSet.Count == 0) return [];

        var tasks = parentStateIds.Select(sId => GetCitiesAsync(sId, lang, ct));
        var results = await Task.WhenAll(tasks);

        var found = results
            .SelectMany(list => list)
            .Where(c => idSet.Contains(c.GeonameId))
            .Select(c => new GeoNamesNamedItem(c.GeonameId, c.Name))
            .ToList();

        // Fall back to individual getJSON for any IDs not found in the city lists
        // (small towns, different feature codes, etc.) — each result is cached 24h
        var foundIds = found.Select(f => f.GeonameId).ToHashSet();
        var missing  = idSet.Where(id => !foundIds.Contains(id)).ToList();
        if (missing.Count > 0)
        {
            var fallbackTasks = missing.Select(id => ResolveByIdAsync(id, lang, ct));
            var fallbacks = await Task.WhenAll(fallbackTasks);
            found.AddRange(fallbacks.Where(f => f is not null)!);
        }

        return found;
    }

    /// <summary>
    /// Resolves a single GeoNames ID to a name via getJSON — cached 24h.
    /// Used as fallback for locations not found in the standard list endpoints.
    /// </summary>
    public Task<GeoNamesNamedItem?> ResolveByIdAsync(int geonameId, string lang = "en", CancellationToken ct = default)
        => GetOrFetchSingleAsync<GeoNamesNamedItem?>($"byid_{geonameId}_{lang}", async () =>
        {
            var url  = $"{_options.BaseUrl}/getJSON?geonameId={geonameId}&username={_options.Username}&lang={lang}";
            var data = await FetchAsync<GeoNamesGetResponse>(url);
            if (data is null) return null;
            var name = string.IsNullOrWhiteSpace(data.Name) ? data.ToponymName : data.Name;
            _logger.LogInformation("Resolved geonameId {Id} [{Lang}] = {Name} — cached 24h", geonameId, lang, name);
            return new GeoNamesNamedItem(geonameId, name);
        });

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<List<T>> GetOrFetchAsync<T>(string key, Func<Task<List<T>>> fetch)
    {
        if (_cache.TryGetValue(key, out List<T>? hit) && hit is not null) return hit;

        var sem = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync();
        try
        {
            if (_cache.TryGetValue(key, out hit) && hit is not null) return hit;
            var result = await fetch();
            _cache.Set(key, result, CacheDuration);
            return result;
        }
        finally { sem.Release(); }
    }

    private async Task<T?> GetOrFetchSingleAsync<T>(string key, Func<Task<T?>> fetch)
    {
        if (_cache.TryGetValue(key, out T? hit)) return hit;

        var sem = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync();
        try
        {
            if (_cache.TryGetValue(key, out hit)) return hit;
            var result = await fetch();
            _cache.Set(key, result, CacheDuration);
            return result;
        }
        finally { sem.Release(); }
    }

    private async Task<T?> FetchAsync<T>(string url)
    {
        // Use CancellationToken.None — we never want a user navigation to abort
        // a GeoNames fetch that would leave the cache empty
        var response = await _httpClient.GetAsync(url, CancellationToken.None);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, _json);
    }

    // ── Response models (internal) ────────────────────────────────────────────

    private class GeoNamesCountryInfoResponse { public List<GeoNamesCountryInfo> Geonames { get; set; } = []; }
    private class GeoNamesCountryInfo  { public int GeonameId { get; set; } public string CountryCode { get; set; } = null!; public string CountryName { get; set; } = null!; }
    private class GeoNamesChildInfo    { public int GeonameId { get; set; } public string Name { get; set; } = null!; public string Fcode { get; set; } = null!; }
    private class GeoNamesResponse<T>  { public List<T> Geonames { get; set; } = []; }
    private class GeoNamesLocationInfo { public int GeonameId { get; set; } public string CountryCode { get; set; } = null!; public string AdminCode1 { get; set; } = null!; public string Fcode { get; set; } = null!; }
    private class GeoNamesSearchResponse { public List<GeoNamesSearchResult> Geonames { get; set; } = []; }
    private class GeoNamesSearchResult { public int GeonameId { get; set; } public string Name { get; set; } = null!; public string Fcode { get; set; } = null!; public int Population { get; set; } }
    private class GeoNamesGetResponse  { public int GeonameId { get; set; } public string Name { get; set; } = null!; public string ToponymName { get; set; } = null!; }
}
