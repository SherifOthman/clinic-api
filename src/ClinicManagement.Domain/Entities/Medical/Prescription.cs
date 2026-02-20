using System;
using System.Collections.Generic;
using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Prescription : AuditableEntity
{
    public string PrescriptionNumber { get; set; } = null!;
    public int VisitId { get; set; }
}
