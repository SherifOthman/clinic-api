using System;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Domain.Identity;

/// <summary>
/// Base application user (IdentityUser)
/// All users have Name, Gender, City stored here
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = null!;
    public string Gender { get; set; } = null!; // Could be Enum if desired
    public string City { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}