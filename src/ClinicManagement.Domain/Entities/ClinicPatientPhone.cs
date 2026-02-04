using System;
using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ClinicPatientPhone : BaseEntity
{
    public Guid ClinicPatientId { get; set; }
    public ClinicPatient ClinicPatient { get; set; } = null!;
    
    public string PhoneNumber { get; set; } = null!;
    public bool IsPrimary { get; set; } = false;
}
