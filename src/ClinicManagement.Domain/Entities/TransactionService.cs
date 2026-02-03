using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Enums;

namespace ClinicManagement.Domain.Entities;

public class TransactionService : AuditableEntity
{
    public Guid PatientTransactionId { get; set; }
    public Guid ServiceId { get; set; }
    public decimal Price { get; set; } // Price at the time of transaction
    public ServiceType Type { get; set; } // Consultation, Operation, Lab Test, etc.
    
    // Navigation properties
    public PatientTransaction PatientTransaction { get; set; } = null!;
    public MedicalService Service { get; set; } = null!;
}