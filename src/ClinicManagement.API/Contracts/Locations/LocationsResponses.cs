namespace ClinicManagement.API.Contracts.Locations;

// ── /api/locations endpoints ──────────────────────────────────────────────────
// Both language names are always returned — the frontend picks which to display.
// No ?lang= param needed, no re-fetching on language switch.

public record CountryResponse(int GeonameId, string NameEn, string NameAr, string CountryCode);
public record StateResponse(int GeonameId, string NameEn, string NameAr);
public record CityResponse(int GeonameId, string NameEn, string NameAr);
