namespace ClinicManagement.Application.Common.Interfaces;

/// <summary>
/// Service for phone number validation using libphonenumber
/// Used internally by validators to ensure phone numbers are valid
/// </summary>
public interface IPhoneValidationService
{
    /// <summary>
    /// Validate and format a phone number
    /// </summary>
    /// <param name="phoneNumber">Phone number to validate</param>
    /// <param name="countryCode">Optional ISO2 country code for better validation</param>
    /// <returns>Validation result with formatted number</returns>
    PhoneValidationResult ValidatePhoneNumber(string phoneNumber, string? countryCode = null);
}

/// <summary>
/// Phone validation result (internal use only)
/// </summary>
public class PhoneValidationResult
{
    public bool IsValid { get; set; }
    public string FormattedNumber { get; set; } = null!;
    public string? CountryCode { get; set; }
    public string? ErrorMessage { get; set; }
}
