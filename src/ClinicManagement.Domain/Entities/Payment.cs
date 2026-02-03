using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class Payment : AuditableEntity
{
    public Guid PatientTransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public PaymentMethod Method { get; set; } // Cash, Card, Transfer...
    public string? Notes { get; set; }
    
    // Navigation properties
    public PatientTransaction PatientTransaction { get; set; } = null!;
}