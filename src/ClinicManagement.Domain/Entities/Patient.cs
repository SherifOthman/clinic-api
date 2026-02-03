using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Identity;

namespace ClinicManagement.Domain.Entities;

public class Patient : BaseEntity
{
    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; } 
    public string FullName { get; set; } = null!;
    public Gender Gender { get; set; }
    public string City { get; set; } = null!;
    public string? Address { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
}