using ClinicManagement.Application.Common.Interfaces;
using PhoneNumbers;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Validates and formats phone numbers using libphonenumber library
/// Used internally by validators to ensure phone numbers are valid
/// </summary>
public class PhoneValidationService : IPhoneValidationService
{
    private readonly PhoneNumberUtil _phoneUtil;

    public PhoneValidationService()
    {
        _phoneUtil = PhoneNumberUtil.GetInstance();
    }

    public PhoneValidationResult ValidatePhoneNumber(string phoneNumber, string? countryCode = null)
    {
        try
        {
            PhoneNumber parsedNumber;
            
            if (!string.IsNullOrEmpty(countryCode))
            {
                parsedNumber = _phoneUtil.Parse(phoneNumber, countryCode);
            }
            else
            {
                if (!phoneNumber.StartsWith("+"))
                {
                    return new PhoneValidationResult
                    {
                        IsValid = false,
                        FormattedNumber = phoneNumber,
                        ErrorMessage = "Phone number must start with + or provide a country code"
                    };
                }
                parsedNumber = _phoneUtil.Parse(phoneNumber, null);
            }

            var isValid = _phoneUtil.IsValidNumber(parsedNumber);
            
            if (!isValid)
            {
                return new PhoneValidationResult
                {
                    IsValid = false,
                    FormattedNumber = phoneNumber,
                    ErrorMessage = "Invalid phone number for the specified country"
                };
            }

            var formattedNumber = _phoneUtil.Format(parsedNumber, PhoneNumberFormat.E164);
            var detectedCountryCode = _phoneUtil.GetRegionCodeForNumber(parsedNumber);

            return new PhoneValidationResult
            {
                IsValid = true,
                FormattedNumber = formattedNumber,
                CountryCode = detectedCountryCode
            };
        }
        catch (NumberParseException ex)
        {
            return new PhoneValidationResult
            {
                IsValid = false,
                FormattedNumber = phoneNumber,
                ErrorMessage = $"Failed to parse phone number: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new PhoneValidationResult
            {
                IsValid = false,
                FormattedNumber = phoneNumber,
                ErrorMessage = $"Validation error: {ex.Message}"
            };
        }
    }
}
