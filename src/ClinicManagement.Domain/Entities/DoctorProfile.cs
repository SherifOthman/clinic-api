using ClinicManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicManagement.Domain.Entities;

public class DoctorProfile : BaseEntity
{
   public Guid StaffId { get; set; }
   public Staff Staff { get; set; } = null!;
   public int SpecializationId { get; set;  }
   public Specialization? Specialization { get; set; }
}
