using System;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class Staff : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public Guid ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;
    
    public StaffRole Role { get; set; }
    
    /// <summary>
    /// Whether this staff member is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Link to doctor profile if role is Doctor
    /// </summary>
    public Guid? DoctorProfileId { get; set; }
    public Doctor? DoctorProfile { get; set; }
}
