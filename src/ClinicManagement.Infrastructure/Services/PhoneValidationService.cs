using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.DTOs;
using PhoneNumbers;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Validates and formats phone numbers using libphonenumber library
/// </summary>
public class PhoneValidationService : IPhoneValidationService
{
    private readonly PhoneNumberUtil _phoneUtil;

    public PhoneValidationService()
    {
        _phoneUtil = PhoneNumberUtil.GetInstance();
    }

    public ValidatePhoneResponse ValidatePhoneNumber(string phoneNumber, string? countryCode = null)
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
                    return new ValidatePhoneResponse
                    {
                        IsValid = false,
                        FormattedNumber = phoneNumber,
                        OriginalNumber = phoneNumber,
                        ErrorMessage = "Phone number must start with + or provide a country code"
                    };
                }
                parsedNumber = _phoneUtil.Parse(phoneNumber, null);
            }

            var isValid = _phoneUtil.IsValidNumber(parsedNumber);
            
            if (!isValid)
            {
                return new ValidatePhoneResponse
                {
                    IsValid = false,
                    FormattedNumber = phoneNumber,
                    OriginalNumber = phoneNumber,
                    ErrorMessage = "Invalid phone number for the specified country"
                };
            }

            var formattedNumber = _phoneUtil.Format(parsedNumber, PhoneNumberFormat.E164);
            var detectedCountryCode = _phoneUtil.GetRegionCodeForNumber(parsedNumber);

            return new ValidatePhoneResponse
            {
                IsValid = true,
                FormattedNumber = formattedNumber,
                OriginalNumber = phoneNumber,
                CountryCode = detectedCountryCode,
                ErrorMessage = null
            };
        }
        catch (NumberParseException ex)
        {
            return new ValidatePhoneResponse
            {
                IsValid = false,
                FormattedNumber = phoneNumber,
                OriginalNumber = phoneNumber,
                ErrorMessage = $"Failed to parse phone number: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new ValidatePhoneResponse
            {
                IsValid = false,
                FormattedNumber = phoneNumber,
                OriginalNumber = phoneNumber,
                ErrorMessage = $"Validation error: {ex.Message}"
            };
        }
    }
}
