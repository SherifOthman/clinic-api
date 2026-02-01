namespace ClinicManagement.Application.DTOs;

public class UserClinicDto
{
    public Guid Id { get; set; }
    public Guid ClinicId { get; set; }
    public string ClinicName { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
    public bool IsActive { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime JoinedAt { get; set; }
    public SubscriptionPlanDto? SubscriptionPlan { get; set; }
}

public class SwitchClinicRequest
{
    public Guid ClinicId { get; set; }
}

public class SwitchClinicResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public UserClinicDto CurrentClinic { get; set; } = null!;
}