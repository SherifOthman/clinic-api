namespace ClinicManagement.API.Common.Exceptions;

/// <summary>
/// Exception thrown when a phone number value is invalid
/// </summary>
public class InvalidPhoneNumberException : DomainException
{
    public InvalidPhoneNumberException(string message, string? errorCode = null) 
        : base(message, errorCode ?? string.Empty)
    {
    }

    public InvalidPhoneNumberException(string message, string? errorCode, Exception innerException) 
        : base(message, errorCode ?? string.Empty, innerException)
    {
    }
}
