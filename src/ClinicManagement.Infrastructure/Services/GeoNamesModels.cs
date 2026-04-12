namespace ClinicManagement.Infrastructure.Services;

/// <summary>Single-language GeoNames models — language is passed per request.</summary>
public record GeoNamesCountry
{
    public int GeonameId { get; init; }
    public string CountryCode { get; init; } = null!;
    public string Name { get; init; } = null!;
}

public record GeoNamesLocation
{
    public int GeonameId { get; init; }
    public string Fcode { get; init; } = null!;
    public string Name { get; init; } = null!;
}
