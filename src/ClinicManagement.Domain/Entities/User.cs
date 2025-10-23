using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Domain.Entities;

public class User : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string? SecondName { get; set; }
    public string ThirdName { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public new string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<Clinic> OwnedClinics { get; set; } = new List<Clinic>();
    public virtual ICollection<Doctor> DoctorProfiles { get; set; } = new List<Doctor>();
    public virtual ICollection<Receptionist> ReceptionistProfiles { get; set; } = new List<Receptionist>();
}
