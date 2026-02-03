namespace ClinicManagement.Domain.Common.Exceptions;

/// <summary>
/// Exception thrown when trying to use expired medicine
/// </summary>
public class ExpiredMedicineException : DomainException
{
    public DateTime ExpiryDate { get; }

    public ExpiredMedicineException(DateTime expiryDate, string errorCode) 
        : base($"Medicine expired on {expiryDate:yyyy-MM-dd}", errorCode)
    {
        ExpiryDate = expiryDate;
    }
}
