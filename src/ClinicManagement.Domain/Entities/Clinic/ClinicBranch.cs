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
    
    public ICollection<DoctorWorkingDay> DoctorWorkingDays { get; set; } = new List<DoctorWorkingDay>();
    public ICollection<ClinicBranchAppointmentPrice> AppointmentPrices { get; set; } = new List<ClinicBranchAppointmentPrice>();
    public ICollection<MedicalService> MedicalServices { get; set; } = new List<MedicalService>();
    public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
    public ICollection<MedicalSupply> MedicalSupplies { get; set; } = new List<MedicalSupply>();
}
