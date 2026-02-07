using ClinicManagement.Application.DTOs;

namespace ClinicManagement.Application.Common.Interfaces;

/// <summary>
/// Service for phone number validation using libphonenumber
/// </summary>
public interface IPhoneValidationService
{
    /// <summary>
    /// Validate and format a phone number
    /// </summary>
    /// <param name="phoneNumber">Phone number to validate</param>
    /// <param name="countryCode">Optional ISO2 country code for better validation</param>
    /// <returns>Validation result with formatted number</returns>
    ValidatePhoneResponse ValidatePhoneNumber(string phoneNumber, string? countryCode = null);

    /// <summary>
    /// Get list of country phone codes
    /// </summary>
    /// <returns>List of countries with their phone codes</returns>
    List<CountryPhoneCodeDto> GetCountryPhoneCodes();
}
