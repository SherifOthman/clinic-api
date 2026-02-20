namespace ClinicManagement.Domain.Entities;

public class User
{
    public int Id { get;  set; }
    public string UserName { get;  set; } = string.Empty;
    public string Email { get;  set; } = string.Empty;
    public string PasswordHash { get;  set; } = string.Empty;
    public string FirstName { get;  set; } = string.Empty;
    public string LastName { get;  set; } = string.Empty;
    public string PhoneNumber { get;  set; } = string.Empty;
    public string? ProfileImageUrl { get;  set; }
    public bool IsEmailConfirmed { get;  set; }

    public string FullName => $"{FirstName} {LastName}".Trim();


}
