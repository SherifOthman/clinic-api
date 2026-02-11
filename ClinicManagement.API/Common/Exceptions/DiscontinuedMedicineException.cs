namespace ClinicManagement.API.Common.Exceptions;

/// <summary>
/// Exception thrown when trying to use discontinued medicine
/// </summary>
public class DiscontinuedMedicineException : DomainException
{
    public DiscontinuedMedicineException(string? errorCode = null) 
        : base("Cannot perform operation on discontinued medicine", errorCode)
    {
    }
}
