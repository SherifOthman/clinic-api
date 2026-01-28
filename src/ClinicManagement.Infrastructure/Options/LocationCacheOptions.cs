namespace ClinicManagement.Infrastructure.Options;

public class LocationCacheOptions
{
    public const string SectionName = "LocationCache";
    
    public int CountriesCacheDurationDays { get; set; } = 30;
    public int StatesCacheDurationDays { get; set; } = 7;
    public int CitiesCacheDurationDays { get; set; } = 1;
    
    public TimeSpan CountriesCacheDuration => TimeSpan.FromDays(CountriesCacheDurationDays);
    public TimeSpan StatesCacheDuration => TimeSpan.FromDays(StatesCacheDurationDays);
    public TimeSpan CitiesCacheDuration => TimeSpan.FromDays(CitiesCacheDurationDays);
}