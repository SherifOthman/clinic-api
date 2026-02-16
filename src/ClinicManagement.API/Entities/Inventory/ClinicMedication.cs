using System;
using ClinicManagement.API.Common;

namespace ClinicManagement.API.Entities;

public class ClinicMedication : TenantEntity
{
    public Clinic Clinic { get; set; } = null!;
    
    public Guid? MedicationId { get; set; }
    public Medication? Medication { get; set; }
    
    public string DisplayName { get; set; } = null!;
}
