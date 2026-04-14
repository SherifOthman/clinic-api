namespace ClinicManagement.API.Contracts.Locations;

// ── /api/locations endpoints (used by LocationSelector form component) ────────

/// <summary>A country item returned by GET /api/locations/countries</summary>
public record CountryResponse(int GeonameId, string Name, string CountryCode);

/// <summary>A state item returned by GET /api/locations/countries/{id}/states</summary>
public record StateResponse(int GeonameId, string Name);

/// <summary>A city item returned by GET /api/locations/states/{id}/cities</summary>
public record CityResponse(int GeonameId, string Name);

// ── /api/patients/location-options (filter dropdowns) ────────────────────────

/// <summary>
/// A single item in a location filter dropdown.
/// GeonameId is the value sent back as a filter param.
/// Name is already resolved in the requested language.
/// </summary>
public record LocationOption(int GeonameId, string Name);
