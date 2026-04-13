namespace ClinicManagement.Domain.Entities;

/// <summary>GeoNames state/governorate — seeded once at startup, bilingual.</summary>
public class GeoState
{
    public int GeonameId        { get; set; }
    public int CountryGeonameId { get; set; }

    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;

    public GeoCountry Country  { get; set; } = null!;
    public ICollection<GeoCity> Cities { get; set; } = [];
}
