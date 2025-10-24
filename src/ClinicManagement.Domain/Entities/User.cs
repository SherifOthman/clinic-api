using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Domain.Entities;

public class User : IdentityUser<int>
{
    public string FirstName { get; set; } = string.Empty;
    public string? SecondName { get; set; }
    public string ThirdName { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
}
