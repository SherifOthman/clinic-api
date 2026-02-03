using System;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Identity;

namespace ClinicManagement.Domain.Entities;

public class Staff : AuditableEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    
    public Guid ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;
    
    public StaffRole Role { get; set; }
    
    /// <summary>
    /// Link to doctor profile if role is Doctor
    /// </summary>
    public Guid? DoctorProfileId { get; set; }
    public DoctorProfile? DoctorProfile { get; set; }
}