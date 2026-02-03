using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicManagement.Domain.Entities;

public class ClinicPatient: AuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid PatientId { get; set; }
    public Clinic? Clinic { get; set; } = null!;
    public Patient? Patient { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set;}
    public Gender Gender {get; set; }
    public string City { get; set; } = string.Empty;
    public List<ChronicDisease> chronicDiseases { get; set; } = new();
    public List<PatientPhone> PhoneNumbers { get; set; } = new();
    
    // New relationships for transactions, visits, and measurements
    public ICollection<PatientTransaction> Transactions { get; set; } = new List<PatientTransaction>();
    public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    public ICollection<PatientMeasurement> Measurements { get; set; } = new List<PatientMeasurement>();
}
