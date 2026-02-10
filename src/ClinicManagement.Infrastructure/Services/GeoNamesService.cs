using System.Text.Json;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Enhanced GeoNames proxy service with BILINGUAL support (Arabic + English)
/// 
/// CRITICAL ARCHITECTURE:
/// - Frontend NEVER calls GeoNames directly
/// - Backend acts as proxy with caching
/// - Fetches data in BOTH languages (EN + AR)
/// - Merges results by GeonameId
/// - Returns unified bilingual DTOs
/// - 24-hour memory cache
/// - Resilient to GeoNames outages
/// </summary>
public partial class GeoNamesService : IGeoNamesService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GeoNamesService> _logger;
    private readonly GeoNamesOptions _options;

    public GeoNamesService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<GeoNamesService> logger,
        IOptions<GeoNamesOptions> options)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<List<GeoNamesCountryDto>> GetCountriesAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "geonames:countries:bilingual";

        if (_cache.TryGetValue(cacheKey, out List<GeoNamesCountryDto>? cachedCountries))
        {
            _logger.LogDebug("Countries retrieved from cache");
            return cachedCountries!;
        }

        _logger.LogInformation("Fetching countries from GeoNames API (bilingual)");

        try
        {
            // Fetch English data
            var countriesEn = await FetchCountriesInLanguageAsync("en", cancellationToken);
            
            // Fetch Arabic data
            var countriesAr = await FetchCountriesInLanguageAsync("ar", cancellationToken);

            // Merge by GeonameId
            var countries = MergeCountryData(countriesEn, countriesAr);

            // Cache with configured duration
            _cache.Set(cacheKey, countries, _options.CountriesCacheDuration);

            _logger.LogInformation("Fetched and cached {Count} countries (bilingual) for {Duration}", 
                countries.Count, _options.CountriesCacheDuration);

            return countries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch countries from GeoNames API");
            throw;
        }
    }

    public async Task<List<GeoNamesLocationDto>> GetStatesAsync(int countryGeonameId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"geonames:states:{countryGeonameId}:bilingual";

        if (_cache.TryGetValue(cacheKey, out List<GeoNamesLocationDto>? cachedStates))
        {
            _logger.LogDebug("States for country {CountryGeonameId} retrieved from cache", countryGeonameId);
            return cachedStates!;
        }

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

            // Cache with configured duration
            _cache.Set(cacheKey, states, _options.StatesCacheDuration);

            _logger.LogInformation("Fetched and cached {Count} states for country {CountryGeonameId} (bilingual) for {Duration}", 
                states.Count, countryGeonameId, _options.StatesCacheDuration);

            return states;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch states for country {CountryGeonameId} from GeoNames API", countryGeonameId);
            throw;
        }
    }

    public async Task<List<GeoNamesLocationDto>> GetCitiesAsync(int stateGeonameId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"geonames:cities:{stateGeonameId}:bilingual";

        if (_cache.TryGetValue(cacheKey, out List<GeoNamesLocationDto>? cachedCities))
        {
            _logger.LogDebug("Cities for state {StateGeonameId} retrieved from cache", stateGeonameId);
            return cachedCities!;
        }

        _logger.LogInformation("Fetching cities for state {StateGeonameId} from GeoNames API (bilingual)", stateGeonameId);

        try
        {
            // Fetch English data
            var citiesEn = await FetchChildrenInLanguageAsync(stateGeonameId, "en", cancellationToken);
            
            // Fetch Arabic data
            var citiesAr = await FetchChildrenInLanguageAsync(stateGeonameId, "ar", cancellationToken);

            // Filter for PPL (populated places) and merge
            var cities = MergeLocationData(
                citiesEn.Where(c => c.Fcode.StartsWith("ADM2")).ToList(),
                citiesAr.Where(c => c.Fcode.StartsWith("ADM2")).ToList()
            );

            // Cache with configured duration
            _cache.Set(cacheKey, cities, _options.CitiesCacheDuration);

            _logger.LogInformation("Fetched and cached {Count} cities for state {StateGeonameId} (bilingual) for {Duration}", 
                cities.Count, stateGeonameId, _options.CitiesCacheDuration);

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
        var url = $"{_options.BaseUrl}/childrenJSON?geonameId={parentGeonameId}&username={_options.Username}&lang={language}";
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<GeoNamesResponse<GeoNamesChildInfo>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return result?.Geonames ?? new List<GeoNamesChildInfo>();
    }

    private List<GeoNamesCountryDto> MergeCountryData(
        List<GeoNamesCountryInfo> englishData,
        List<GeoNamesCountryInfo> arabicData)
    {
        var arabicDict = arabicData.ToDictionary(c => c.GeonameId, c => c.CountryName);

        return englishData
            .Select(en => new GeoNamesCountryDto
            {
                GeonameId = en.GeonameId,
                CountryCode = en.CountryCode,
                PhoneCode = en.CountryCode, // Will be enhanced with actual phone codes
                Name = new BilingualName
                {
                    En = en.CountryName,
                    Ar = arabicDict.TryGetValue(en.GeonameId, out var arName) ? arName : en.CountryName
                }
            })
            .OrderBy(c => c.Name.En)
            .ToList();
    }

    private List<GeoNamesLocationDto> MergeLocationData(
        List<GeoNamesChildInfo> englishData,
        List<GeoNamesChildInfo> arabicData)
    {
        var arabicDict = arabicData.ToDictionary(c => c.GeonameId, c => c.Name);

        return englishData
            .Select(en => new GeoNamesLocationDto
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

    #endregion
}
