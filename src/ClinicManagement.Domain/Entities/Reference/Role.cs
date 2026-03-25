using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Domain.Entities;

public class Role : IdentityRole<Guid>
{
    public Role()
    {
        Id = Guid.NewGuid();
    }

    public Role(string roleName) : this()
    {
        Name = roleName;
        NormalizedName = roleName.ToUpperInvariant();
    }

    public string? Description { get; set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
}

public static class UserRoles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string ClinicOwner = "ClinicOwner";
    public const string Doctor = "Doctor";
    public const string Receptionist = "Receptionist";
}
