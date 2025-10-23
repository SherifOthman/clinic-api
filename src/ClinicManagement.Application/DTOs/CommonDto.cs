namespace ClinicManagement.Application.DTOs;

public class CountryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? FlagUrl { get; set; }
}

public class CreateCountryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? FlagUrl { get; set; }
}

public class GovernorateDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CountryId { get; set; }
    public CountryDto? Country { get; set; }
}

public class CreateGovernorateDto
{
    public string Name { get; set; } = string.Empty;
    public int CountryId { get; set; }
}

public class CityDto
{
    public int Id { get; set; }
    public int GovernorateId { get; set; }
    public GovernorateDto? Governorate { get; set; }
}

public class CreateCityDto
{
    public int GovernorateId { get; set; }
}

public class SubscriptionPlanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DoctorLimit { get; set; }
    public int AppointmentLimit { get; set; }
    public int DurationDays { get; set; }
    public string? Features { get; set; }
}

public class CreateSubscriptionPlanDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DoctorLimit { get; set; }
    public int AppointmentLimit { get; set; }
    public int DurationDays { get; set; }
    public string? Features { get; set; }
}
