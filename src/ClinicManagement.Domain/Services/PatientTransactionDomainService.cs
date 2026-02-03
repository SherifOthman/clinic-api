using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Services;

/// <summary>
/// Domain service implementing PatientTransaction business logic
/// </summary>
public class PatientTransactionDomainService : IPatientTransactionDomainService
{
    public decimal CalculateTotalAmount(PatientTransaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));
            
        var servicesTotal = transaction.Services?.Sum(s => s.Price) ?? 0;
        var itemsTotal = transaction.Items?.Sum(i => i.Quantity * i.UnitPrice) ?? 0;
        
        return servicesTotal + itemsTotal;
    }
    
    public decimal CalculatePaidAmount(PatientTransaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));
            
        return transaction.Payments?.Sum(p => p.Amount) ?? 0;
    }
    
    public decimal CalculateRemainingAmount(PatientTransaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));
            
        return CalculateTotalAmount(transaction) - CalculatePaidAmount(transaction);
    }
    
    public bool IsFullyPaid(PatientTransaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));
            
        return CalculateRemainingAmount(transaction) <= 0;
    }
    
    public bool IsOverpaid(PatientTransaction transaction)
    {
        if (transaction == null)
            throw new ArgumentNullException(nameof(transaction));
            
        return CalculateRemainingAmount(transaction) < 0;
    }
}