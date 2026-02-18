using System;
using System.Collections.Generic;
using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class ClinicBranch : TenantEntity
{
    public Clinic Clinic { get; set; } = null!;
    
    public string Name { get; set; } = null!;
    
    public int CountryGeoNameId { get; set; }
    public int StateGeoNameId { get; set; }
    public int CityGeoNameId { get; set; }
    
    public string AddressLine { get; set; } = null!;
    
    public ICollection<ClinicBranchPhoneNumber> PhoneNumbers { get; set; } = new List<ClinicBranchPhoneNumber>();
    public ICollection<DoctorWorkingDay> DoctorWorkingDays { get; set; } = new List<DoctorWorkingDay>();
    public ICollection<ClinicBranchAppointmentPrice> AppointmentPrices { get; set; } = new List<ClinicBranchAppointmentPrice>();
    public ICollection<MedicalService> MedicalServices { get; set; } = new List<MedicalService>();
    public ICollection<Medicine> Medicines { get; set; } = new List<Medicine>();
    public ICollection<MedicalSupply> MedicalSupplies { get; set; } = new List<MedicalSupply>();
}
