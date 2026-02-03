using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicManagement.Domain.Entities;

public class WorkingDay: BaseEntity
{
    public Guid ClinicBranchId { get; set;  }
    public ClinicBranch ClinicBranch { get; set; } = null!;
    public WeekDay Day { get; set; }
    public bool IsWorkingDay { get;set;  }
}
