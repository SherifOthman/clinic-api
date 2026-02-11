using System;
using System.Collections.Generic;
using ClinicManagement.API.Common;

namespace ClinicManagement.API.Entities;

public class Prescription : AuditableEntity
{
    public Guid VisitId { get; set; }
    public MedicalVisit Visit { get; set; } = null!;

    public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();

}
