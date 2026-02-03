using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Services;

/// <summary>
/// Domain service for PatientTransaction business logic
/// </summary>
public interface IPatientTransactionDomainService
{
    /// <summary>
    /// Calculates the total amount for a transaction
    /// </summary>
    decimal CalculateTotalAmount(PatientTransaction transaction);
    
    /// <summary>
    /// Calculates the total paid amount for a transaction
    /// </summary>
    decimal CalculatePaidAmount(PatientTransaction transaction);
    
    /// <summary>
    /// Calculates the remaining amount for a transaction
    /// </summary>
    decimal CalculateRemainingAmount(PatientTransaction transaction);
    
    /// <summary>
    /// Checks if a transaction is fully paid
    /// </summary>
    bool IsFullyPaid(PatientTransaction transaction);
    
    /// <summary>
    /// Checks if a transaction is overpaid
    /// </summary>
    bool IsOverpaid(PatientTransaction transaction);
}