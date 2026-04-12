namespace ClinicManagement.API.Contracts.Locations;

/// <summary>Single-language location responses — language is determined by the ?lang= query param.</summary>
public record CountryResponse(int GeonameId, string Name, string CountryCode);
public record StateResponse(int GeonameId, string Name);
public record CityResponse(int GeonameId, string Name);
