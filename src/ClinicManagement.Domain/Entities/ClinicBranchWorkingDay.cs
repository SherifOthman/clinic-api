using System;
using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Stores which days branch works (e.g., Monday to Friday)
/// </summary>
public class ClinicBranchWorkingDay : BaseEntity
{
    public Guid ClinicBranchId { get; set; }
    public ClinicBranch ClinicBranch { get; set; } = null!;
    
    public DayOfWeek Day { get; set; }
    public bool IsOpen { get; set; } = true;
}