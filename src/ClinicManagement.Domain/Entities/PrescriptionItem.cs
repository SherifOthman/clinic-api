using System;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class PrescriptionItem : BaseEntity
{
    public Guid PrescriptionId { get; set; }
    public Prescription Prescription { get; set; } = null!;
    
    public ServiceType ItemType { get; set; } // Medication / Lab / Radiology
    public Guid ReferenceId { get; set; } // points to actual item
    public string Instructions { get; set; } = null!;
}