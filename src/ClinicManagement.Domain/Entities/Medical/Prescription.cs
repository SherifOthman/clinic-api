using System;
using System.Collections.Generic;
using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class Prescription : AuditableEntity
{
    public int VisitId { get; set; }

}
