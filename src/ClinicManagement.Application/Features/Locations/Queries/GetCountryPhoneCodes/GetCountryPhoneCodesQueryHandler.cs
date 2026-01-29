using ClinicManagement.Application.Common.Constants;
using ClinicManagement.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ClinicManagement.Application.Features.Locations.Queries.GetCountryPhoneCodes;

public class GetCountryPhoneCodesQueryHandler : IRequestHandler<GetCountryPhoneCodesQuery, Result<List<CountryPhoneCodeDto>>>
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GetCountryPhoneCodesQueryHandler> _logger;
    private const string CacheKey = "country-phone-codes";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public GetCountryPhoneCodesQueryHandler(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<GetCountryPhoneCodesQueryHandler> logger)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<List<CountryPhoneCodeDto>>> Handle(GetCountryPhoneCodesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Check cache first
            if (_cache.TryGetValue(CacheKey, out List<CountryPhoneCodeDto>? cached))
            {
                return Result<List<CountryPhoneCodeDto>>.Ok(cached!);
            }

            // Fetch from REST Countries API
            var response = await _httpClient.GetAsync("https://restcountries.com/v3.1/all?fields=name,cca2,idd,flag", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch country data from REST Countries API. Status: {StatusCode}", response.StatusCode);
                return Result<List<CountryPhoneCodeDto>>.Fail(MessageCodes.Location.COUNTRIES_API_FAILED);
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var countries = JsonSerializer.Deserialize<RestCountryResponse[]>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (countries == null)
            {
                return Result<List<CountryPhoneCodeDto>>.Fail(MessageCodes.Location.COUNTRIES_API_INVALID_RESPONSE);
            }

            var result = countries
                .Where(c => !string.IsNullOrEmpty(c.Cca2) && 
                           c.Name?.Common != null && 
                           c.Idd?.Root != null)
                .Select(c => new CountryPhoneCodeDto
                {
                    Name = c.Name!.Common!,
                    Code = c.Cca2!,
                    PhoneCode = c.Idd!.Root + (c.Idd.Suffixes?.FirstOrDefault() ?? ""),
                    Flag = c.Flag ?? ""
                })
                .Where(c => !string.IsNullOrEmpty(c.PhoneCode) && c.PhoneCode != "+")
                .OrderBy(c => c.Name)
                .ToList();

            // Cache the result
            _cache.Set(CacheKey, result, CacheDuration);

            return Result<List<CountryPhoneCodeDto>>.Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching country phone codes");
            return Result<List<CountryPhoneCodeDto>>.Fail(MessageCodes.Location.COUNTRIES_API_FAILED);
        }
    }
}

// Response models for REST Countries API
public class RestCountryResponse
{
    public RestCountryName? Name { get; set; }
    public string? Cca2 { get; set; }
    public RestCountryIdd? Idd { get; set; }
    public string? Flag { get; set; }
}

public class RestCountryName
{
    public string? Common { get; set; }
}

public class RestCountryIdd
{
    public string? Root { get; set; }
    public string[]? Suffixes { get; set; }
}