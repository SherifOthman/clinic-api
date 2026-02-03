using System;
using System.Collections.Generic;
using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ClinicBranch : AuditableEntity
{
    public Guid ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;
    
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    
    public ICollection<ClinicBranchWorkingDay> WorkingDays { get; set; } = new List<ClinicBranchWorkingDay>();
}