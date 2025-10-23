using ClinicManagement.Application.DTOs;

namespace ClinicManagement.Application.DTOs;

public class ClinicDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public int OwnerId { get; set; }
    public int SubscriptionPlanId { get; set; }
    public UserDto? Owner { get; set; }
    public SubscriptionPlanDto? SubscriptionPlan { get; set; }
    public List<ClinicBranchDto> Branches { get; set; } = new();
}

public class CreateClinicDto
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public int OwnerId { get; set; }
    public int SubscriptionPlanId { get; set; }
}

public class UpdateClinicDto
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
}
