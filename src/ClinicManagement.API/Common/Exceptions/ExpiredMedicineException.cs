namespace ClinicManagement.API.Common.Exceptions;

public class ExpiredMedicineException : DomainException
{
    public DateTime ExpiryDate { get; }

    public ExpiredMedicineException(DateTime expiryDate) 
        : base("MEDICINE_EXPIRED", $"Medicine expired on {expiryDate:yyyy-MM-dd}")
    {
        ExpiryDate = expiryDate;
    }
}
