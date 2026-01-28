using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Infrastructure.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;

namespace ClinicManagement.Infrastructure.Services;

public class GeoNamesLocationService : ILocationsService
{
    private readonly IGeoNamesClient _client;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GeoNamesLocationService> _logger;
    private readonly LocationCacheOptions _cacheOptions;

    public GeoNamesLocationService(
        IGeoNamesClient client, 
        IMemoryCache cache, 
        ILogger<GeoNamesLocationService> logger,
        IOptions<LocationCacheOptions> cacheOptions)
    {
        _client = client;
        _cache = cache;
        _logger = logger;
        _cacheOptions = cacheOptions.Value;
    }

    public async Task<List<CountryDto>> GetCountriesAsync()
    {
        return await GetFromCacheOrApi(
            cacheKey: "countries",
            cacheDuration: _cacheOptions.CountriesCacheDuration,
            apiCall: () => FetchCountriesFromApi(),
            errorMessage: "Failed to get countries"
        );
    }

    public async Task<List<StateDto>> GetStatesAsync(int countryId)
    {
        var country = await GetCountryById(countryId);
        if (country == null) return new List<StateDto>();

        return await GetFromCacheOrApi(
            cacheKey: $"states_{countryId}",
            cacheDuration: _cacheOptions.StatesCacheDuration,
            apiCall: () => FetchStatesFromApi(country.Code!, countryId),
            errorMessage: $"Failed to get states for country {countryId}"
        );
    }

    public async Task<List<CityDto>> GetCitiesAsync(int countryId, int? stateId = null)
    {
        if (!stateId.HasValue) return new List<CityDto>();

        var country = await GetCountryById(countryId);
        var state = await GetStateById(countryId, stateId.Value);
        if (country == null || state == null) return new List<CityDto>();

        return await GetFromCacheOrApi(
            cacheKey: $"cities_{countryId}_{stateId}",
            cacheDuration: _cacheOptions.CitiesCacheDuration,
            apiCall: () => FetchCitiesFromApi(country.Code!, countryId, stateId.Value, state.Name),
            errorMessage: $"Failed to get cities for country {countryId}, state {stateId}"
        );
    }

    public async Task<List<CityDto>> SearchCitiesAsync(string countryCode, string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            return new List<CityDto>();

        try
        {
            var request = new GeoNamesSearchRequest
            {
                Query = query,
                CountryCode = countryCode,
                MaxResults = 50,
                FeatureClass = "P"
            };

            var results = await _client.SearchAsync(request);
            var country = await GetCountryByCodeAsync(countryCode);
            return results.Select(r => CreateCityDto(r, country?.Id ?? 0, null)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search cities for query {Query} in country {CountryCode}", query, countryCode);
            return new List<CityDto>();
        }
    }

    public async Task<CountryDto?> GetCountryByCodeAsync(string countryCode)
    {
        var countries = await GetCountriesAsync();
        return countries.FirstOrDefault(c => c.Code?.Equals(countryCode, StringComparison.OrdinalIgnoreCase) == true);
    }

    private async Task<T> GetFromCacheOrApi<T>(string cacheKey, TimeSpan cacheDuration, Func<Task<T>> apiCall, string errorMessage)
    {
        if (_cache.TryGetValue(cacheKey, out T? cached))
            return cached!;

        try
        {
            var result = await apiCall();
            _cache.Set(cacheKey, result, cacheDuration);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, errorMessage);
            
            // Return empty list of correct type
            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
            {
                var listType = typeof(T).GetGenericArguments()[0];
                var emptyList = Activator.CreateInstance(typeof(List<>).MakeGenericType(listType));
                return (T)emptyList!;
            }
            
            return default(T)!;
        }
    }

    private async Task<CountryDto?> GetCountryById(int countryId)
    {
        var countries = await GetCountriesAsync();
        return countries.FirstOrDefault(c => c.Id == countryId);
    }

    private async Task<StateDto?> GetStateById(int countryId, int stateId)
    {
        var states = await GetStatesAsync(countryId);
        return states.FirstOrDefault(s => s.Id == stateId);
    }

    private async Task<List<CountryDto>> FetchCountriesFromApi()
    {
        var request = new GeoNamesSearchRequest
        {
            Query = "*",
            MaxResults = 250,
            FeatureClass = "A",
            FeatureCode = "PCLI"
        };

        var results = await _client.SearchAsync(request);
        return results
            .Where(r => !string.IsNullOrEmpty(r.CountryCode) && !string.IsNullOrEmpty(r.Name))
            .Select(r => new CountryDto
            {
                Id = r.GeoNameId,
                Name = CleanName(r.Name),
                Code = r.CountryCode,
                PhoneCode = ""
            })
            .OrderBy(c => c.Name)
            .ToList();
    }

    private async Task<List<StateDto>> FetchStatesFromApi(string countryCode, int countryId)
    {
        var request = new GeoNamesSearchRequest
        {
            CountryCode = countryCode,
            MaxResults = 100,
            FeatureClass = "A",
            FeatureCode = "ADM1"
        };

        var results = await _client.SearchAsync(request);
        return results
            .Where(r => !string.IsNullOrEmpty(r.AdminName1))
            .GroupBy(r => r.AdminName1)
            .Select(g => CreateStateDto(g.First(), countryId))
            .Where(s => !string.IsNullOrEmpty(s.Name))
            .OrderBy(s => s.Name)
            .ToList();
    }

    private async Task<List<CityDto>> FetchCitiesFromApi(string countryCode, int countryId, int stateId, string stateName)
    {
        var request = new GeoNamesSearchRequest
        {
            CountryCode = countryCode,
            MaxResults = 1000,
            FeatureClass = "P"
        };

        var results = await _client.SearchAsync(request);
        return results
            .Where(r => !string.IsNullOrEmpty(r.AdminName1) && 
                       r.AdminName1.Contains(stateName, StringComparison.OrdinalIgnoreCase))
            .Select(r => CreateCityDto(r, countryId, stateId))
            .Where(c => !string.IsNullOrEmpty(c.Name))
            .GroupBy(c => c.Name.ToLowerInvariant())
            .Select(g => g.First())
            .OrderBy(c => c.Name)
            .ToList();
    }

    private static StateDto CreateStateDto(GeoNamesLocationDto geoName, int countryId)
    {
        return new StateDto
        {
            Id = geoName.GeoNameId,
            Name = CleanName(geoName.AdminName1 ?? geoName.Name),
            CountryId = countryId
        };
    }

    private static CityDto CreateCityDto(GeoNamesLocationDto geoName, int countryId, int? stateId)
    {
        return new CityDto
        {
            Id = geoName.GeoNameId,
            Name = CleanName(geoName.Name),
            CountryId = countryId,
            StateId = stateId,
            Latitude = geoName.Latitude,
            Longitude = geoName.Longitude
        };
    }

    private static string CleanName(string name)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;

        // Remove diacritics
        var normalized = name.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();
        
        foreach (char c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(c);
        }
        
        var cleaned = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        
        // Remove quotes and apostrophes
        cleaned = Regex.Replace(cleaned, @"['`'""\u2018\u2019\u201C\u201D]", "");
        
        return cleaned.Trim();
    }
}