namespace ClinicManagement.API.Common.Exceptions;

public class DiscontinuedMedicineException : DomainException
{
    public DiscontinuedMedicineException(string message = "Cannot perform operation on discontinued medicine") 
        : base("MEDICINE_DISCONTINUED", message)
    {
    }
}
