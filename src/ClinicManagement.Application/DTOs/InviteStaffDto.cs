using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Application.DTOs;

public class InviteStaffDto
{
    public string Email { get; set; } = null!;
    public UserType UserType { get; set; } // Doctor or Receptionist
}

public class StaffInvitationDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public UserType UserType { get; set; }
    public string InvitedByUserName { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsAccepted { get; set; }
    public DateTime? AcceptedAt { get; set; }
}

public class AcceptInvitationDto
{
    public string Token { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? UserName { get; set; }
    public string Password { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? ProfileImageUrl { get; set; }
}
