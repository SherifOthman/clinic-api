using System;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

/// <summary>
/// Services done during the visit (operations, lab, radiology, medications)
/// </summary>
public class VisitServiceItem : AuditableEntity
{
    public Guid VisitId { get; set; }
    public Visit Visit { get; set; } = null!;
    
    public ServiceType ServiceType { get; set; }
    public Guid? ReferenceId { get; set; } // points to Medication/Lab/Radiology/Procedure
    
    public decimal Price { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount => Price - DiscountAmount - PaidAmount;
    
    public PaymentStatus PaymentStatus =>
        RemainingAmount == 0 ? PaymentStatus.Paid :
        PaidAmount > 0 ? PaymentStatus.PartiallyPaid :
        PaymentStatus.Unpaid;
}