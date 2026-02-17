using System;
using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ClinicMedication : TenantEntity
{
    public Clinic Clinic { get; set; } = null!;
    
    public Guid? MedicationId { get; set; }
    public Medication? Medication { get; set; }
    
    public string DisplayName { get; set; } = null!;
}
