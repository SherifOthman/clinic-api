using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicManagement.Domain.Entities;

public class Appointment : AuditableEntity
{
    public Guid ClinicBranchId { get; set;  }
    public ClinicBranch ClinicBranch { get; set; } = null!;
    public Guid ClinicPatientId  { get; set; }
    public ClinicPatient ClinicPatient { get; set; } = null!;
    public Guid StaffId { get; set; }
    public Staff? Staff { get; set; } = null!; // Doctor
    public DateTime AppointmentDate { get; set; }
    public short QueueNumber { get; set; }
    public AppointmentStatus Status { get; set; }
    public VisitType VisitType { get; set; }
    //public decimal Price { get; set; }
    //public decimal DiscountAmount { get; set; }
    //public decimal PaidAmount { get; set; }
    //public decimal RemainingAmount => Price - DiscountAmount - PaidAmount;


    // Domain logic
    public void Confirm()
    {
        if (Status != AppointmentStatus.Pending)
            throw new DomainException("Only pending appointments can be confirmed.");
        Status = AppointmentStatus.Confirmed;
    }

    public void Complete()
    {
        if (Status != AppointmentStatus.Confirmed)
            throw new DomainException("Only confirmed appointments can be completed.");
        Status = AppointmentStatus.Completed;
    }

    public void Cancel()
    {
        if (Status == AppointmentStatus.Completed)
            throw new DomainException("Cannot cancel completed appointment.");
        Status = AppointmentStatus.Cancelled;
    }
}
