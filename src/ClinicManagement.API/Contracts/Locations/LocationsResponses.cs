namespace ClinicManagement.API.Contracts.Locations;

/// <summary>Single-language location responses — language is determined by the ?lang= query param.</summary>
public record CountryResponse(int GeonameId, string Name, string CountryCode);
public record StateResponse(int GeonameId, string Name);
public record CityResponse(int GeonameId, string Name);

/// <summary>A resolved GeoNames ID → name pair.</summary>
public record LocationNameDto(int GeonameId, string Name, int? ParentGeonameId = null);

/// <summary>
/// Combined response for the patient location filter.
/// Returns all three lists (countries, states, cities) with names resolved in one round trip.
/// </summary>
public record PatientLocationFilterResponse(
    List<LocationNameDto> Countries,
    List<LocationNameDto> States,
    List<LocationNameDto> Cities);
