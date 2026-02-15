using System;
using ClinicManagement.API.Common;

namespace ClinicManagement.API.Entities;

/// <summary>
/// Stores working days for each doctor at each clinic branch
/// </summary>
public class DoctorWorkingDay : BaseEntity
{
    public Guid DoctorId { get; set; }
    public DoctorProfile DoctorProfile { get; set; } = null!;
    
    public Guid ClinicBranchId { get; set; }
    public ClinicBranch ClinicBranch { get; set; } = null!;
    
    public DayOfWeek Day { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;
}
