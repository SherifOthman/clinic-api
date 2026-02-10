namespace ClinicManagement.Application.DTOs;

public class CompleteOnboardingDto
{
    public string ClinicName { get; set; } = null!;
    public string SubscriptionPlanId { get; set; } = null!;
    public string BranchName { get; set; } = null!;
    public string BranchAddress { get; set; } = null!;
    
    // GeoNames location data (only IDs - names fetched from cache/GeoNames)
    public LocationDataDto Location { get; set; } = null!;
    
    public List<BranchPhoneNumberDto> BranchPhoneNumbers { get; set; } = new();
}

/// <summary>
/// Location data with GeoNames IDs only
/// Names are fetched from cache or GeoNames API when needed
/// </summary>
public class LocationDataDto
{
    // Country
    public int CountryGeonameId { get; set; }
    public string CountryIso2Code { get; set; } = null!;
    public string CountryPhoneCode { get; set; } = null!;
    
    // State
    public int StateGeonameId { get; set; }
    
    // City
    public int CityGeonameId { get; set; }
}

public class BranchPhoneNumberDto
{
    public string PhoneNumber { get; set; } = null!;
    public string? Label { get; set; }
}
