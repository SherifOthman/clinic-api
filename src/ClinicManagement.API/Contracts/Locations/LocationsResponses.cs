namespace ClinicManagement.API.Contracts.Locations;

// ── /api/locations endpoints (used by LocationSelector form component) ────────

/// <summary>A country item returned by GET /api/locations/countries</summary>
public record CountryResponse(int GeonameId, string Name, string CountryCode);

/// <summary>A state item returned by GET /api/locations/countries/{id}/states</summary>
public record StateResponse(int GeonameId, string Name);

/// <summary>A city item returned by GET /api/locations/states/{id}/cities</summary>
public record CityResponse(int GeonameId, string Name);
