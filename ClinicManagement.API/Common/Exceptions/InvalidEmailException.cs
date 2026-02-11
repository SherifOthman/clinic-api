namespace ClinicManagement.API.Common.Exceptions;

public class InvalidEmailException : DomainException
{
    public InvalidEmailException(string message) 
        : base("INVALID_EMAIL", message)
    {
    }

    public InvalidEmailException(string message, Exception innerException) 
        : base("INVALID_EMAIL", message, innerException)
    {
    }
}
