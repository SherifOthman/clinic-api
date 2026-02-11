namespace ClinicManagement.API.Common.Exceptions;

/// <summary>
/// Exception thrown when an email value is invalid
/// </summary>
public class InvalidEmailException : DomainException
{
    public InvalidEmailException(string message, string? errorCode = null) 
        : base(message, errorCode ?? string.Empty)
    {
    }

    public InvalidEmailException(string message, string? errorCode, Exception innerException) 
        : base(message, errorCode ?? string.Empty, innerException)
    {
    }
}
