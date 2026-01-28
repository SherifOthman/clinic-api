using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Web;

namespace ClinicManagement.Infrastructure.Services;

public class GeoNamesApiClient : IGeoNamesClient
{
    private readonly HttpClient _httpClient;
    private readonly GeoNamesOptions _options;
    private readonly ILogger<GeoNamesApiClient> _logger;

    public GeoNamesApiClient(
        HttpClient httpClient,
        IOptions<GeoNamesOptions> options,
        ILogger<GeoNamesApiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.Timeout = _options.RequestTimeout;
    }

    private static decimal ParseDecimal(string? value)
    {
        return decimal.TryParse(value, out var result) ? result : 0;
    }

    private static GeoNamesLocationDto ConvertToDto(GeoNamesLocation location)
    {
        return new GeoNamesLocationDto
        {
            GeoNameId = location.GeonameId,
            Name = location.Name ?? string.Empty,
            CountryCode = location.CountryCode ?? string.Empty,
            AdminName1 = location.AdminName1,
            Latitude = ParseDecimal(location.Lat),
            Longitude = ParseDecimal(location.Lng)
        };
    }

    public async Task<List<GeoNamesLocationDto>> SearchAsync(GeoNamesSearchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = BuildSearchQueryParams(request);
            var response = await _httpClient.GetAsync($"searchJSON?{queryParams}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<GeoNamesSearchResponse>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return result?.Geonames?.Select(ConvertToDto).ToList() ?? new List<GeoNamesLocationDto>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GeoNames search failed for query: {Query}", request.Query);
            return new List<GeoNamesLocationDto>();
        }
    }

    public async Task<GeoNamesLocationDto?> GetLocationByIdAsync(int geoNameId, CancellationToken cancellationToken = default)
    {
        try
        {
            var queryParams = $"geonameId={geoNameId}&username={_options.Username}";
            var response = await _httpClient.GetAsync($"getJSON?{queryParams}", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<GeoNamesLocation>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return result != null ? ConvertToDto(result) : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "GeoNames get location by ID failed for ID: {GeoNameId}", geoNameId);
            return null;
        }
    }

    private string BuildSearchQueryParams(GeoNamesSearchRequest request)
    {
        var queryParams = new List<string>
        {
            $"q={HttpUtility.UrlEncode(request.Query)}",
            $"maxRows={request.MaxResults}",
            $"username={_options.Username}"
        };

        if (!string.IsNullOrEmpty(request.CountryCode))
            queryParams.Add($"country={request.CountryCode}");

        if (!string.IsNullOrEmpty(request.FeatureClass))
            queryParams.Add($"featureClass={request.FeatureClass}");

        if (!string.IsNullOrEmpty(request.FeatureCode))
            queryParams.Add($"featureCode={request.FeatureCode}");

        if (!string.IsNullOrEmpty(request.AdminCode1))
            queryParams.Add($"adminCode1={request.AdminCode1}");

        return string.Join("&", queryParams);
    }
}

// Simplified response models for GeoNames API
public class GeoNamesSearchResponse
{
    public List<GeoNamesLocation>? Geonames { get; set; }
}

public class GeoNamesLocation
{
    public int GeonameId { get; set; }
    public string? Name { get; set; }
    public string? Lat { get; set; }
    public string? Lng { get; set; }
    public string? CountryCode { get; set; }
    public string? AdminName1 { get; set; }
}