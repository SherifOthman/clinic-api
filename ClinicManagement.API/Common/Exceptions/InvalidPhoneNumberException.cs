namespace ClinicManagement.API.Common.Exceptions;

public class InvalidPhoneNumberException : DomainException
{
    public InvalidPhoneNumberException(string message) 
        : base("INVALID_PHONE", message)
    {
    }
}
