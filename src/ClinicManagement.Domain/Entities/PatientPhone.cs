using ClinicManagement.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicManagement.Domain.Entities;

public class PatientPhone :BaseEntity
{
   public Guid ClinicPatientId { get; set; }
   public ClinicPatient clinicPatient { get; set; } = null!;
   public string PhoneNumber { get; set; } = string.Empty;
}
