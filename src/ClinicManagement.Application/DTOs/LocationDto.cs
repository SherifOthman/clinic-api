namespace ClinicManagement.Application.DTOs;

public class CountryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneCode { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty; // ISO2 code for API calls
}

public class StateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CountryId { get; set; }
}

public class CityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CountryId { get; set; }
    public int? StateId { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
}
