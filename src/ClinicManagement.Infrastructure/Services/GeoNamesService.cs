using System.Text.Json;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// GeoNames proxy service with memory caching
/// Acts as backend proxy to protect GeoNames rate limits
/// Frontend communicates ONLY with this service, NEVER directly with GeoNames
/// </summary>
public class GeoNamesService : IGeoNamesService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GeoNamesService> _logger;
    private const string GeoNamesUsername = "sheriff_ali";
    private const string BaseUrl = "http://api.geonames.org";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public GeoNamesService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<GeoNamesService> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<GeoNamesCountryDto>> GetCountriesAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = "geonames:countries";

        // Try get from cache
        if (_cache.TryGetValue(cacheKey, out List<GeoNamesCountryDto>? cachedCountries))
        {
            _logger.LogDebug("Countries retrieved from cache");
            return cachedCountries!;
        }

        _logger.LogInformation("Fetching countries from GeoNames API");

        try
        {
            var url = $"{BaseUrl}/countryInfoJSON?username={GeoNamesUsername}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<GeoNamesCountryInfoResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Geonames == null || result.Geonames.Count == 0)
            {
                _logger.LogWarning("No countries returned from GeoNames API");
                return new List<GeoNamesCountryDto>();
            }

            var countries = result.Geonames
                .Select(c => new GeoNamesCountryDto
                {
                    GeonameId = c.GeonameId,
                    CountryCode = c.CountryCode,
                    CountryName = c.CountryName,
                    CountryNameAr = null // GeoNames doesn't provide Arabic names in country info
                })
                .OrderBy(c => c.CountryName)
                .ToList();

            // Cache for 24 hours
            _cache.Set(cacheKey, countries, CacheDuration);

            _logger.LogInformation("Fetched and cached {Count} countries from GeoNames", countries.Count);

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
        var cacheKey = $"geonames:states:{countryGeonameId}";

        // Try get from cache
        if (_cache.TryGetValue(cacheKey, out List<GeoNamesLocationDto>? cachedStates))
        {
            _logger.LogDebug("States for country {CountryGeonameId} retrieved from cache", countryGeonameId);
            return cachedStates!;
        }

        _logger.LogInformation("Fetching states for country {CountryGeonameId} from GeoNames API", countryGeonameId);

        try
        {
            var url = $"{BaseUrl}/childrenJSON?geonameId={countryGeonameId}&username={GeoNamesUsername}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<GeoNamesResponse<GeoNamesLocationDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Geonames == null || result.Geonames.Count == 0)
            {
                _logger.LogWarning("No states returned from GeoNames API for country {CountryGeonameId}", countryGeonameId);
                return new List<GeoNamesLocationDto>();
            }

            var states = result.Geonames
                .Where(s => s.Fcode.StartsWith("ADM1")) // Administrative division level 1
                .OrderBy(s => s.Name)
                .ToList();

            // Cache for 24 hours
            _cache.Set(cacheKey, states, CacheDuration);

            _logger.LogInformation("Fetched and cached {Count} states for country {CountryGeonameId}", states.Count, countryGeonameId);

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
        var cacheKey = $"geonames:cities:{stateGeonameId}";

        // Try get from cache
        if (_cache.TryGetValue(cacheKey, out List<GeoNamesLocationDto>? cachedCities))
        {
            _logger.LogDebug("Cities for state {StateGeonameId} retrieved from cache", stateGeonameId);
            return cachedCities!;
        }

        _logger.LogInformation("Fetching cities for state {StateGeonameId} from GeoNames API", stateGeonameId);

        try
        {
            var url = $"{BaseUrl}/childrenJSON?geonameId={stateGeonameId}&username={GeoNamesUsername}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<GeoNamesResponse<GeoNamesLocationDto>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Geonames == null || result.Geonames.Count == 0)
            {
                _logger.LogWarning("No cities returned from GeoNames API for state {StateGeonameId}", stateGeonameId);
                return new List<GeoNamesLocationDto>();
            }

            var cities = result.Geonames
                .Where(c => c.Fcode.StartsWith("PPL")) // Populated places (cities/towns)
                .OrderBy(c => c.Name)
                .ToList();

            // Cache for 24 hours
            _cache.Set(cacheKey, cities, CacheDuration);

            _logger.LogInformation("Fetched and cached {Count} cities for state {StateGeonameId}", cities.Count, stateGeonameId);

            return cities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch cities for state {StateGeonameId} from GeoNames API", stateGeonameId);
            throw;
        }
    }

    // Internal response models for GeoNames API
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
}
