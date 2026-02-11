using System.Text.Json;
using ClinicManagement.API.Common.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.API.Infrastructure.Services;

/// <summary>
/// GeoNames API proxy with bilingual support (Arabic + English)
/// - Fetches location data in both languages
/// - Merges by GeonameId
/// - Caching handled by Output Cache at endpoint level
/// - Frontend never calls GeoNames directly
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

    public async Task<List<GeoNamesCountryDto>> GetCountriesAsync(CancellationToken cancellationToken = default)
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

    public async Task<List<GeoNamesLocationDto>> GetStatesAsync(int countryGeonameId, CancellationToken cancellationToken = default)
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

    public async Task<List<GeoNamesLocationDto>> GetCitiesAsync(int stateGeonameId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching cities for state {StateGeonameId} from GeoNames API (bilingual)", stateGeonameId);

        try
        {
            // Fetch English data
            var citiesEn = await FetchChildrenInLanguageAsync(stateGeonameId, "en", cancellationToken);
            
            // Fetch Arabic data
            var citiesAr = await FetchChildrenInLanguageAsync(stateGeonameId, "ar", cancellationToken);

            // Filter for ADM2 (administrative division level 2) and merge
            var cities = MergeLocationData(
                citiesEn.Where(c => c.Fcode.StartsWith("ADM2")).ToList(),
                citiesAr.Where(c => c.Fcode.StartsWith("ADM2")).ToList()
            );

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

    /// <summary>
    /// Merges English and Arabic country data by GeonameId
    /// Falls back to English name if Arabic translation not found
    /// </summary>
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

    private class GeoNamesResponse<T>
    {
        public List<T> Geonames { get; set; } = new();
    }

    #endregion
}

// Public DTOs
public record GeoNamesCountryDto
{
    public int GeonameId { get; init; }
    public string CountryCode { get; init; } = null!;
    public string PhoneCode { get; init; } = null!;
    public BilingualName Name { get; init; } = null!;
}

public record GeoNamesLocationDto
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
