using ClinicManagement.Domain.Common;

namespace ClinicManagement.Domain.Entities;

public class PatientTransaction : AuditableEntity
{
    public Guid ClinicId { get; set; }
    public Guid ClinicPatientId { get; set; } // Reference to ClinicPatient instead of Patient directly
    public Guid? VisitId { get; set; } // Optional - linked to a specific visit
    
    // Navigation properties
    public ClinicPatient ClinicPatient { get; set; } = null!;
    public Visit? Visit { get; set; }
    public Clinic Clinic { get; set; } = null!;
    
    // Transaction details
    public ICollection<TransactionService> Services { get; set; } = new List<TransactionService>();
    public ICollection<TransactionItem> Items { get; set; } = new List<TransactionItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    
    // Business logic moved to Domain Service
    // Use IPatientTransactionDomainService.CalculateTotalAmount(this)
    // Use IPatientTransactionDomainService.CalculatePaidAmount(this)  
    // Use IPatientTransactionDomainService.CalculateRemainingAmount(this)
}