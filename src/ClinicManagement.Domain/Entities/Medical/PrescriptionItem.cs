using System;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class PrescriptionItem : BaseEntity
{
    public Guid PrescriptionId { get; set; }
    public Prescription Prescription { get; set; } = null!;

    public string DrugName { get; set; } = null!;

    public string? Dosage { get; set; } 

    public int FrequencyPerDay { get; set; } 

    public int DurationInDays { get; set; } 

    public string? Instructions { get; set; } 
}
