namespace ClinicManagement.Application.DTOs;

public class CompleteOnboardingDto
{
    public string ClinicName { get; set; } = null!;
    public string SubscriptionPlanId { get; set; } = null!;
    public string BranchName { get; set; } = null!;
    public string BranchAddress { get; set; } = null!;
    
    // GeoNames location data (received from client)
    public LocationDataDto Location { get; set; } = null!;
    
    public List<BranchPhoneNumberDto> BranchPhoneNumbers { get; set; } = new();
}

/// <summary>
/// Location data with GeoNames IDs and names
/// Client sends this after selecting location from GeoNames API
/// </summary>
public class LocationDataDto
{
    // Country
    public int CountryGeonameId { get; set; }
    public string CountryIso2Code { get; set; } = null!;
    public string CountryPhoneCode { get; set; } = null!;
    public string CountryNameEn { get; set; } = null!;
    public string CountryNameAr { get; set; } = null!;
    
    // State
    public int StateGeonameId { get; set; }
    public string StateNameEn { get; set; } = null!;
    public string StateNameAr { get; set; } = null!;
    
    // City
    public int CityGeonameId { get; set; }
    public string CityNameEn { get; set; } = null!;
    public string CityNameAr { get; set; } = null!;
}

public class BranchPhoneNumberDto
{
    public string PhoneNumber { get; set; } = null!;
    public string? Label { get; set; }
}
