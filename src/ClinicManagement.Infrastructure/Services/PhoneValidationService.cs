using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Application.DTOs;
using PhoneNumbers;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Phone validation service using libphonenumber
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
            // Try to parse the phone number
            PhoneNumber parsedNumber;
            
            if (!string.IsNullOrEmpty(countryCode))
            {
                // Parse with country code hint
                parsedNumber = _phoneUtil.Parse(phoneNumber, countryCode);
            }
            else
            {
                // Try to parse as international format (must start with +)
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

            // Validate the parsed number
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

            // Format to E.164
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

    public List<CountryPhoneCodeDto> GetCountryPhoneCodes()
    {
        // Get supported regions from libphonenumber
        var supportedRegions = _phoneUtil.GetSupportedRegions();
        
        var phoneCodes = new List<CountryPhoneCodeDto>();

        foreach (var region in supportedRegions)
        {
            try
            {
                var countryCode = _phoneUtil.GetCountryCodeForRegion(region);
                if (countryCode == 0) continue; // Skip invalid regions

                var countryName = GetCountryName(region);
                var flag = GetFlagEmoji(region);

                phoneCodes.Add(new CountryPhoneCodeDto
                {
                    Name = countryName,
                    Code = region,
                    PhoneCode = $"+{countryCode}",
                    Flag = flag
                });
            }
            catch
            {
                // Skip regions that cause errors
                continue;
            }
        }

        // Sort by country name
        return phoneCodes.OrderBy(c => c.Name).ToList();
    }

    private static string GetCountryName(string countryCode)
    {
        // Map of ISO2 codes to country names (common ones)
        var countryNames = new Dictionary<string, string>
        {
            { "EG", "Egypt" },
            { "SA", "Saudi Arabia" },
            { "AE", "United Arab Emirates" },
            { "KW", "Kuwait" },
            { "QA", "Qatar" },
            { "BH", "Bahrain" },
            { "OM", "Oman" },
            { "JO", "Jordan" },
            { "LB", "Lebanon" },
            { "SY", "Syria" },
            { "IQ", "Iraq" },
            { "YE", "Yemen" },
            { "PS", "Palestine" },
            { "US", "United States" },
            { "GB", "United Kingdom" },
            { "CA", "Canada" },
            { "AU", "Australia" },
            { "DE", "Germany" },
            { "FR", "France" },
            { "IT", "Italy" },
            { "ES", "Spain" },
            { "NL", "Netherlands" },
            { "BE", "Belgium" },
            { "CH", "Switzerland" },
            { "AT", "Austria" },
            { "SE", "Sweden" },
            { "NO", "Norway" },
            { "DK", "Denmark" },
            { "FI", "Finland" },
            { "PL", "Poland" },
            { "CZ", "Czech Republic" },
            { "HU", "Hungary" },
            { "RO", "Romania" },
            { "BG", "Bulgaria" },
            { "GR", "Greece" },
            { "PT", "Portugal" },
            { "IE", "Ireland" },
            { "TR", "Turkey" },
            { "IN", "India" },
            { "CN", "China" },
            { "JP", "Japan" },
            { "KR", "South Korea" },
            { "BR", "Brazil" },
            { "MX", "Mexico" },
            { "AR", "Argentina" },
            { "CL", "Chile" },
            { "CO", "Colombia" },
            { "PE", "Peru" },
            { "VE", "Venezuela" },
            { "ZA", "South Africa" },
            { "NG", "Nigeria" },
            { "KE", "Kenya" },
            { "MA", "Morocco" },
            { "TN", "Tunisia" },
            { "DZ", "Algeria" },
            { "LY", "Libya" },
            { "SD", "Sudan" },
            { "ET", "Ethiopia" },
            { "GH", "Ghana" },
            { "UG", "Uganda" },
            { "TZ", "Tanzania" },
            { "RU", "Russia" },
            { "UA", "Ukraine" },
            { "BY", "Belarus" },
            { "KZ", "Kazakhstan" },
            { "UZ", "Uzbekistan" },
            { "PK", "Pakistan" },
            { "BD", "Bangladesh" },
            { "LK", "Sri Lanka" },
            { "NP", "Nepal" },
            { "AF", "Afghanistan" },
            { "IR", "Iran" },
            { "TH", "Thailand" },
            { "VN", "Vietnam" },
            { "MY", "Malaysia" },
            { "SG", "Singapore" },
            { "ID", "Indonesia" },
            { "PH", "Philippines" },
            { "NZ", "New Zealand" }
        };

        return countryNames.TryGetValue(countryCode, out var name) ? name : countryCode;
    }

    private static string GetFlagEmoji(string countryCode)
    {
        // Convert ISO2 country code to flag emoji
        // Each letter is converted to its regional indicator symbol
        if (string.IsNullOrEmpty(countryCode) || countryCode.Length != 2)
            return "🏳️";

        // Ensure uppercase
        countryCode = countryCode.ToUpper();
        
        // Convert each character to regional indicator symbol
        var firstChar = char.ConvertFromUtf32(0x1F1E6 + (countryCode[0] - 'A'));
        var secondChar = char.ConvertFromUtf32(0x1F1E6 + (countryCode[1] - 'A'));
        return firstChar + secondChar;
    }
}
