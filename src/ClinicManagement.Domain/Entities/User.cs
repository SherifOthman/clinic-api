using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Domain.Entities;

public class User : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int? ClinicId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Clinic? Clinic { get; set; }
    public virtual Clinic? OwnedClinic { get; set; }
    public virtual Doctor? Doctor { get; set; }
    public virtual Receptionist? Receptionist { get; set; }
}
