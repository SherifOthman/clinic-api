using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicManagement.Domain.Entities.Appointment;

public class QueueNumberCounter
{
    public Guid DoctorId { get; set; }
    public Guid ClinicBranchId { get; set; }
    public DateOnly Date { get; set; }
    public int Value { get; set; }
}

