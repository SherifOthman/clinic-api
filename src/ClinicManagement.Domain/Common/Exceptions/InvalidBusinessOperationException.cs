namespace ClinicManagement.Domain.Common.Exceptions;

/// <summary>
/// Exception thrown when a business operation is invalid
/// </summary>
public class InvalidBusinessOperationException : DomainException
{
    public InvalidBusinessOperationException(string message, string? errorCode = null) 
        : base(message, errorCode ?? string.Empty)
    {
    }

    public InvalidBusinessOperationException(string message, string? errorCode, Exception innerException) 
        : base(message, errorCode ?? string.Empty, innerException)
    {
    }
}
