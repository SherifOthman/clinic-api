using System;
using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class DoctorWorkingDay : BaseEntity
{
    public Guid DoctorId { get; set; }
    public Guid ClinicBranchId { get; set; }
    public DayOfWeek Day { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;

    // Navigation properties
    public DoctorProfile DoctorProfile { get; set; } = null!;
    public ClinicBranch ClinicBranch { get; set; } = null!;
}
