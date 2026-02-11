namespace ClinicManagement.API.Common.Exceptions;

public class InvalidBusinessOperationException : DomainException
{
    public InvalidBusinessOperationException(string message) 
        : base("OPERATION_NOT_ALLOWED", message)
    {
    }
}
