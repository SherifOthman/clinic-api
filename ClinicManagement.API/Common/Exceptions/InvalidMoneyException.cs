namespace ClinicManagement.API.Common.Exceptions;

/// <summary>
/// Exception thrown when a money value or operation is invalid
/// </summary>
public class InvalidMoneyException : DomainException
{
    public InvalidMoneyException(string message, string? errorCode = null) 
        : base(message, errorCode ?? string.Empty)
    {
    }

    public InvalidMoneyException(string message, string? errorCode, Exception innerException) 
        : base(message, errorCode ?? string.Empty, innerException)
    {
    }
}
