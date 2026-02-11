namespace ClinicManagement.API.Common.Exceptions;

/// <summary>
/// Exception thrown when domain validation fails
/// </summary>
public class DomainValidationException : DomainException
{
    public Dictionary<string, List<string>> ValidationErrors { get; }

    public DomainValidationException(string field, string message) 
        : base("VALIDATION_ERROR", message)
    {
        ValidationErrors = new Dictionary<string, List<string>>
        {
            { field, new List<string> { message } }
        };
    }

    public DomainValidationException(Dictionary<string, List<string>> validationErrors) 
        : base("VALIDATION_ERROR", "Validation failed")
    {
        ValidationErrors = validationErrors;
    }
}
