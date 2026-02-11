namespace ClinicManagement.API.Common.Exceptions;

public class InvalidMoneyException : DomainException
{
    public InvalidMoneyException(string message) 
        : base("INVALID_FORMAT", message)
    {
    }
}
