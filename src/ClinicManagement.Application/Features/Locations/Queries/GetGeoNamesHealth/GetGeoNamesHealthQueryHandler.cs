using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Services;
using ClinicManagement.Application.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Application.Features.Locations.Queries.GetGeoNamesHealth;

public class GetGeoNamesHealthQueryHandler : IRequestHandler<GetGeoNamesHealthQuery, Result<GeoNamesHealthDto>>
{
    private readonly ILocationsService _locationsService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<GetGeoNamesHealthQueryHandler> _logger;

    public GetGeoNamesHealthQueryHandler(
        ILocationsService locationsService,
        IDateTimeProvider dateTimeProvider,
        ILogger<GetGeoNamesHealthQueryHandler> logger)
    {
        _locationsService = locationsService;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<GeoNamesHealthDto>> Handle(GetGeoNamesHealthQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var startTime = _dateTimeProvider.UtcNow;
            
            // Test GeoNames service availability by making a simple request
            bool webServiceAvailable = false;
            string? responseTime = null;
            var errors = new Dictionary<string, string>();

            try
            {
                // Try to get countries to test the service
                var countries = await _locationsService.GetCountriesAsync();
                webServiceAvailable = countries.Any();
                responseTime = (_dateTimeProvider.UtcNow - startTime).TotalMilliseconds.ToString("F0") + "ms";
            }
            catch (Exception ex)
            {
                webServiceAvailable = false;
                errors.Add("WebService", ex.Message);
                _logger.LogWarning(ex, "GeoNames web service health check failed");
            }

            var healthDto = new GeoNamesHealthDto
            {
                WebServiceAvailable = webServiceAvailable,
                WebServiceResponseTime = responseTime,
                LastSuccessfulSync = webServiceAvailable ? _dateTimeProvider.UtcNow : null,
                Errors = errors
            };

            _logger.LogInformation("GeoNames health check completed. Available: {Available}", webServiceAvailable);
            return Result<GeoNamesHealthDto>.Ok(healthDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing GeoNames health check");
            return Result<GeoNamesHealthDto>.Fail(MessageCodes.Location.GEONAMES_HEALTH_FAILED);
        }
    }
}
