using System;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Identity;

namespace ClinicManagement.Domain.Entities;

public class Patient : AuditableEntity
{
    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    public string FullName { get; set; } = null!;
    public string Gender { get; set; } = null!;
    public string City { get; set; } = null!;
    public string Address { get; set; } = null!;
    public DateTime DateOfBirth { get; set; }
}