namespace ClinicManagement.Application.DTOs;

public class ClinicDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SubscriptionPlanId { get; set; }
    public string SubscriptionPlanName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UserCount { get; set; }
    public int PatientCount { get; set; }
}

public class CreateClinicDto
{
    public string Name { get; set; } = string.Empty;
    public int SubscriptionPlanId { get; set; }
}
