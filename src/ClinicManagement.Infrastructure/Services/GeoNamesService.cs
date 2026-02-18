using System.Text.Json;
using ClinicManagement.Application.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// GeoNames API proxy with bilingual support (Arabic + English)
/// - Fetches location data in both languages
/// - Merges by GeonameId
/// - Caching handled by Output Cache at endpoint level
/// - Frontend never calls GeoNames directly
/// - Cities filtered by population (>= 5000) to exclude tiny villages
/// </summary>
public partial class GeoNamesService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeoNamesService> _logger;
    private readonly GeoNamesOptions _options;

    public GeoNamesService(
        HttpClient httpClient,
        ILogger<GeoNamesService> logger,
        IOptions<GeoNamesOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<List<GeoNamesCountry>> GetCountriesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching countries from GeoNames API (bilingual)");

        try
        {
            // Fetch English data
            var countriesEn = await FetchCountriesInLanguageAsync("en", cancellationToken);
            
            // Fetch Arabic data
            var countriesAr = await FetchCountriesInLanguageAsync("ar", cancellationToken);

            // Merge by GeonameId
            var countries = MergeCountryData(countriesEn, countriesAr);

            _logger.LogInformation("Fetched {Count} countries (bilingual)", countries.Count);

            return countries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch countries from GeoNames API");
            throw;
        }
    }

    public async Task<List<GeoNamesLocation>> GetStatesAsync(int countryGeonameId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching states for country {CountryGeonameId} from GeoNames API (bilingual)", countryGeonameId);

        try
        {
            // Fetch English data
            var statesEn = await FetchChildrenInLanguageAsync(countryGeonameId, "en", cancellationToken);
            
            // Fetch Arabic data
            var statesAr = await FetchChildrenInLanguageAsync(countryGeonameId, "ar", cancellationToken);

            // Filter for ADM1 (administrative division level 1) and merge
            var states = MergeLocationData(
                statesEn.Where(s => s.Fcode.StartsWith("ADM1")).ToList(),
                statesAr.Where(s => s.Fcode.StartsWith("ADM1")).ToList()
            );

            _logger.LogInformation("Fetched {Count} states for country {CountryGeonameId} (bilingual)", 
                states.Count, countryGeonameId);

            return states;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch states for country {CountryGeonameId} from GeoNames API", countryGeonameId);
            throw;
        }
    }

    public async Task<List<GeoNamesLocation>> GetCitiesAsync(int stateGeonameId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching cities for state {StateGeonameId} from GeoNames API (bilingual)", stateGeonameId);

        try
        {
            // First, get the state info to extract adminCode1
            var stateInfoEn = await GetLocationInfoAsync(stateGeonameId, "en", cancellationToken);
            
            if (stateInfoEn == null)
            {
                _logger.LogWarning("State {StateGeonameId} not found", stateGeonameId);
                return new List<GeoNamesLocation>();
            }

            // Use search API to get ALL populated places within the state
            // This includes cities, towns, villages that children API might miss
            var citiesEn = await SearchCitiesInStateAsync(
                stateInfoEn.CountryCode, 
                stateInfoEn.AdminCode1, 
                "en", 
                cancellationToken);
            
            var citiesAr = await SearchCitiesInStateAsync(
                stateInfoEn.CountryCode, 
                stateInfoEn.AdminCode1, 
                "ar", 
                cancellationToken);

            // Merge bilingual data
            var cities = MergeLocationData(citiesEn, citiesAr);

            _logger.LogInformation("Fetched {Count} cities for state {StateGeonameId} (bilingual)", 
                cities.Count, stateGeonameId);

            return cities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch cities for state {StateGeonameId} from GeoNames API", stateGeonameId);
            throw;
        }
    }

    #region Private Helper Methods

    private async Task<List<GeoNamesCountryInfo>> FetchCountriesInLanguageAsync(
        string language, 
        CancellationToken cancellationToken)
    {
        var url = $"{_options.BaseUrl}/countryInfoJSON?username={_options.Username}&lang={language}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<GeoNamesCountryInfoResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.Geonames ?? new List<GeoNamesCountryInfo>();
    }

    private async Task<List<GeoNamesChildInfo>> FetchChildrenInLanguageAsync(
        int parentGeonameId,
        string language,
        CancellationToken cancellationToken)
    {
        // Use maxRows=1000 to get all children (GeoNames API allows up to 1000)
        // Default is only 200 which may not include all cities/districts
        var url = $"{_options.BaseUrl}/childrenJSON?geonameId={parentGeonameId}&username={_options.Username}&lang={language}&maxRows=1000";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<GeoNamesResponse<GeoNamesChildInfo>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.Geonames ?? new List<GeoNamesChildInfo>();
    }

    private async Task<GeoNamesLocationInfo?> GetLocationInfoAsync(
        int geonameId,
        string language,
        CancellationToken cancellationToken)
    {
        var url = $"{_options.BaseUrl}/getJSON?geonameId={geonameId}&username={_options.Username}&lang={language}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<GeoNamesLocationInfo>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result;
    }

    private async Task<List<GeoNamesChildInfo>> SearchCitiesInStateAsync(
        string countryCode,
        string adminCode1,
        string language,
        CancellationToken cancellationToken)
    {
        // Use search API to get populated places and administrative divisions
        // Ordered by population to prioritize important cities
        var url = $"{_options.BaseUrl}/searchJSON?country={countryCode}&adminCode1={adminCode1}" +
                  $"&featureClass=P&featureClass=A" + // P=populated places, A=administrative
                  $"&username={_options.Username}&lang={language}&maxRows=1000&orderby=population";
        
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<GeoNamesSearchResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Filter strategy:
        // - Always include ADM2 (administrative divisions)
        // - Always include PPLA2, PPLA3, PPLA4 (administrative capitals)
        // - Include PPL only if population >= 5000 (filters out tiny villages)
        return result?.Geonames?
            .Where(g => g.Fcode.StartsWith("ADM2") || 
                       g.Fcode == "PPLA2" || 
                       g.Fcode == "PPLA3" || 
                       g.Fcode == "PPLA4" ||
                       (g.Fcode == "PPL" && g.Population >= 5000))
            .Select(g => new GeoNamesChildInfo
            {
                GeonameId = g.GeonameId,
                Name = g.Name,
                Fcode = g.Fcode
            })
            .ToList() ?? new List<GeoNamesChildInfo>();
    }

    /// <summary>
    /// Merges English and Arabic country data by GeonameId
    /// Falls back to English name if Arabic translation not found
    /// </summary>
    private List<GeoNamesCountry> MergeCountryData(
        List<GeoNamesCountryInfo> englishData,
        List<GeoNamesCountryInfo> arabicData)
    {
        var arabicDict = arabicData.ToDictionary(c => c.GeonameId, c => c.CountryName);

        return englishData
            .Select(en => new GeoNamesCountry
            {
                GeonameId = en.GeonameId,
                CountryCode = en.CountryCode,
                PhoneCode = en.CountryCode,
                Name = new BilingualName
                {
                    En = en.CountryName,
                    Ar = arabicDict.TryGetValue(en.GeonameId, out var arName) ? arName : en.CountryName
                }
            })
            .OrderBy(c => c.Name.En)
            .ToList();
    }

    /// <summary>
    /// Merges English and Arabic location data (states/cities) by GeonameId
    /// Falls back to English name if Arabic translation not found
    /// </summary>
    private List<GeoNamesLocation> MergeLocationData(
        List<GeoNamesChildInfo> englishData,
        List<GeoNamesChildInfo> arabicData)
    {
        var arabicDict = arabicData.ToDictionary(c => c.GeonameId, c => c.Name);

        return englishData
            .Select(en => new GeoNamesLocation
            {
                GeonameId = en.GeonameId,
                Fcode = en.Fcode,
                Name = new BilingualName
                {
                    En = en.Name,
                    Ar = arabicDict.TryGetValue(en.GeonameId, out var arName) ? arName : en.Name
                }
            })
            .OrderBy(l => l.Name.En)
            .ToList();
    }

    #endregion

    #region Internal Response Models

    private class GeoNamesCountryInfoResponse
    {
        public List<GeoNamesCountryInfo> Geonames { get; set; } = new();
    }

    private class GeoNamesCountryInfo
    {
        public int GeonameId { get; set; }
        public string CountryCode { get; set; } = null!;
        public string CountryName { get; set; } = null!;
    }

    private class GeoNamesChildInfo
    {
        public int GeonameId { get; set; }
        public string Name { get; set; } = null!;
        public string Fcode { get; set; } = null!;
    }

    private class GeoNamesResponse<T>
    {
        public List<T> Geonames { get; set; } = new();
    }

    private class GeoNamesLocationInfo
    {
        public int GeonameId { get; set; }
        public string Name { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public string AdminCode1 { get; set; } = null!;
        public string Fcode { get; set; } = null!;
    }

    private class GeoNamesSearchResponse
    {
        public List<GeoNamesSearchResult> Geonames { get; set; } = new();
    }

    private class GeoNamesSearchResult
    {
        public int GeonameId { get; set; }
        public string Name { get; set; } = null!;
        public string Fcode { get; set; } = null!;
        public int Population { get; set; }
    }

    #endregion
}

// Public response types
public record GeoNamesCountry
{
    public int GeonameId { get; init; }
    public string CountryCode { get; init; } = null!;
    public string PhoneCode { get; init; } = null!;
    public BilingualName Name { get; init; } = null!;
}

public record GeoNamesLocation
{
    public int GeonameId { get; init; }
    public string Fcode { get; init; } = null!;
    public BilingualName Name { get; init; } = null!;
}

public record BilingualName
{
    public string En { get; init; } = null!;
    public string Ar { get; init; } = null!;
}
