namespace ClinicManagement.Application.DTOs;

public class ClinicDto
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public bool IsActive { get; set; }
    public bool OnboardingCompleted { get; set; }
    public string? OnboardingStep { get; set; }
    public List<ClinicBranchDto>? Branches { get; set; }
}

public class CreateClinicDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
}

public class UpdateClinicDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Logo { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateOnboardingDto
{
    public string OnboardingStep { get; set; } = string.Empty;
    public bool OnboardingCompleted { get; set; }
}

public class ClinicBranchDto
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int? CityId { get; set; }
    public string? CityName { get; set; }
    public string? CountryName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public bool IsMainBranch { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateClinicBranchDto
{
    public int ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int? CityId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public bool IsMainBranch { get; set; }
}

public class UpdateClinicBranchDto
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public int? CityId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public bool? IsMainBranch { get; set; }
    public bool? IsActive { get; set; }
}
