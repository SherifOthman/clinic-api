namespace ClinicManagement.Domain.Entities;

/// <summary>GeoNames city — seeded once at startup, bilingual.</summary>
public class GeoCity
{
    public int GeonameId      { get; set; }
    public int StateGeonameId { get; set; }

    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;

    public GeoState State { get; set; } = null!;
}
