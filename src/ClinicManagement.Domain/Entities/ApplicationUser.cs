using System;
using Microsoft.AspNetCore.Identity;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Identity;

/// <summary>
/// Base application user (IdentityUser)
/// All users have Name, Gender, City stored here
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = null!;
    public Gender Gender { get; set; }
    public string City { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}