using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace ClinicManagement.Application.Features.Locations.Queries.GetPhoneCodes;

public class GetPhoneCodesQueryHandler : IRequestHandler<GetPhoneCodesQuery, Result<List<CountryPhoneCodeDto>>>
{
    private readonly IPhoneValidationService _phoneValidationService;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "phone-codes";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public GetPhoneCodesQueryHandler(
        IPhoneValidationService phoneValidationService,
        IMemoryCache cache)
    {
        _phoneValidationService = phoneValidationService;
        _cache = cache;
    }

    public Task<Result<List<CountryPhoneCodeDto>>> Handle(GetPhoneCodesQuery request, CancellationToken cancellationToken)
    {
        // Try to get from cache
        if (_cache.TryGetValue(CacheKey, out List<CountryPhoneCodeDto>? cachedCodes) && cachedCodes != null)
        {
            return Task.FromResult(Result<List<CountryPhoneCodeDto>>.Ok(cachedCodes));
        }

        // Get from service
        var phoneCodes = _phoneValidationService.GetCountryPhoneCodes();

        // Cache the result
        _cache.Set(CacheKey, phoneCodes, CacheDuration);

        return Task.FromResult(Result<List<CountryPhoneCodeDto>>.Ok(phoneCodes));
    }
}
