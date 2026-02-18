using System;
using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ClinicMedication : TenantEntity
{
    
    public int? MedicationId { get; set; }
    public Medication? Medication { get; set; }
}
