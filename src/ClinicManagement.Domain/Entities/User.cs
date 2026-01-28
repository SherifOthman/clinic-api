using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Domain.Entities;

public class User : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int? ClinicId { get; set; }
    public int? SpecializationId { get; set; }
    
    public string? Country { get; set; }
    public string? City { get; set; }
    
    public string? ProfileImageUrl { get; set; }
    public string? ProfileImageFileName { get; set; }
    public DateTime? ProfileImageUpdatedAt { get; set; }
    
    public virtual Clinic? Clinic { get; set; }
    public virtual Specialization? Specialization { get; set; }
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
