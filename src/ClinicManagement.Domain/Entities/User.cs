using ClinicManagement.Domain.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Domain.Entities;

public class User : IdentityUser<int>
{
    public int? ClinicId { get; set; }  // Multi-tenancy: Which clinic this user belongs to (nullable until onboarding)
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Clinic? Clinic { get; set; }
}
