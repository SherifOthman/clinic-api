namespace ClinicManagement.Domain.Common.Exceptions;

/// <summary>
/// Exception thrown when trying to use discontinued medicine
/// </summary>
public class DiscontinuedMedicineException : DomainException
{
    public DiscontinuedMedicineException(string errorCode) 
        : base("Cannot perform operation on discontinued medicine", errorCode)
    {
    }
}
