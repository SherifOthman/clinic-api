namespace ClinicManagement.Domain.Entities;

/// <summary>GeoNames country — seeded once at startup, bilingual.</summary>
public class GeoCountry
{
    /// <summary>GeoNames integer ID (e.g. 357994 for Egypt).</summary>
    public int GeonameId { get; set; }

    public string CountryCode { get; set; } = null!;   // ISO 3166-1 alpha-2 (e.g. "EG")
    public string NameEn      { get; set; } = null!;
    public string NameAr      { get; set; } = null!;

    public ICollection<GeoState> States { get; set; } = [];
}
