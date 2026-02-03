using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicManagement.Domain.Entities;

public class Staff : BaseEntity
{
   public Guid UserId { get; set; }
   public User User { get; set; } = null!;
   public Guid ClinicId { get; set; }
   public Clinic Clinic { get; set; } = null!;
   public StaffRole Role { get; set; }
   public DoctorProfile? DoctorProfile { get; set; }
   public bool IsDoctor ()=> Role == StaffRole.Doctor;
   public bool IsReceptionist ()=> Role == StaffRole.Receptionist;
   public bool IsClinicOwner()=> Role == StaffRole.ClinicOwner;
}
