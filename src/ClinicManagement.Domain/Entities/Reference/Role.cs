namespace ClinicManagement.Domain.Entities;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; }
}

public static class UserRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string ClinicOwner = "ClinicOwner";
    public const string Doctor = "Doctor";
    public const string Receptionist = "Receptionist";
}
