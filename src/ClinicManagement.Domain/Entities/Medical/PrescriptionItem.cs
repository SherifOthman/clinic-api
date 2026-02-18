using System;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Enums;

namespace ClinicManagement.Domain.Entities;

public class PrescriptionItem : BaseEntity
{
    public int PrescriptionId { get; set; }

    public string? Dosage { get; set; } 

    public int FrequencyPerDay { get; set; } 

    public int DurationInDays { get; set; } 

    public string? Instructions { get; set; } 
}
