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
    
    /// <summary>
    /// Location - Store only CityId, derive Country and State through joins
    /// This follows the snapshot architecture pattern to avoid data duplication
    /// </summary>
    public int CityId { get; set; }
    public City City { get; set; } = null!;
    
    public ICollection<ClinicBranchPhoneNumber> PhoneNumbers { get; set; } = new List<ClinicBranchPhoneNumber>();
    public ICollection<DoctorWorkingDay> DoctorWorkingDays { get; set; } = new List<DoctorWorkingDay>();
    public ICollection<ClinicBranchAppointmentPrice> AppointmentPrices { get; set; } = new List<ClinicBranchAppointmentPrice>();
    public ICollection<MedicalService> MedicalServices { get; set; } = new List<MedicalService>();
    public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
    public ICollection<MedicalSupply> MedicalSupplies { get; set; } = new List<MedicalSupply>();
}
