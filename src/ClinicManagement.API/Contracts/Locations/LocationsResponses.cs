namespace ClinicManagement.API.Contracts.Locations;

// ── /api/locations endpoints (used by LocationSelector form component) ────────

/// <summary>A country item returned by GET /api/locations/countries</summary>
public record CountryResponse(int GeonameId, string Name, string CountryCode);

/// <summary>A state item returned by GET /api/locations/countries/{id}/states</summary>
public record StateResponse(int GeonameId, string Name);

/// <summary>A city item returned by GET /api/locations/states/{id}/cities</summary>
public record CityResponse(int GeonameId, string Name);

// ── /api/patients/location-filter endpoint (used by patient list filters) ─────

/// <summary>
/// A country with its resolved name — used in the patient filter dropdown.
/// </summary>
public record FilterCountry(int GeonameId, string Name);

/// <summary>
/// A state with its resolved name and parent country ID.
/// CountryGeonameId is used by the frontend to show only states that belong
/// to the currently selected country filter.
/// </summary>
public record FilterState(int GeonameId, string Name, int CountryGeonameId);

/// <summary>
/// A city with its resolved name and parent state ID.
/// StateGeonameId is used by the frontend to show only cities that belong
/// to the currently selected state filter.
/// </summary>
public record FilterCity(int GeonameId, string Name, int StateGeonameId);

/// <summary>
/// All three location lists in one response — used by the patient list filter.
/// One API call loads everything the filter dropdowns need.
/// </summary>
public record PatientLocationFilterResponse(
    List<FilterCountry> Countries,
    List<FilterState> States,
    List<FilterCity> Cities);
