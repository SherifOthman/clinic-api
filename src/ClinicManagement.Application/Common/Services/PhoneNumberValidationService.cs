using PhoneNumbers;
using System.Text.RegularExpressions;

namespace ClinicManagement.Application.Common.Services;

public interface IPhoneNumberValidationService
{
    bool IsValidPhoneNumber(string phoneNumber, string? defaultRegion = null);
    string FormatPhoneNumber(string phoneNumber, string? defaultRegion = null);
    PhoneNumberValidationResult ValidatePhoneNumber(string phoneNumber, string? defaultRegion = null);
    string GetE164Format(string phoneNumber, string? defaultRegion = null);
    string GetInternationalFormat(string phoneNumber, string? defaultRegion = null);
    string GetNationalFormat(string phoneNumber, string? defaultRegion = null);
    bool IsEgyptianPhoneNumber(string phoneNumber);
}

public class PhoneNumberValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string? CountryCode { get; set; }
    public string? RegionCode { get; set; }
    public PhoneNumberType? NumberType { get; set; }
    public string? E164Format { get; set; }
    public string? InternationalFormat { get; set; }
    public string? NationalFormat { get; set; }
}

public class PhoneNumberValidationService : IPhoneNumberValidationService
{
    private readonly PhoneNumberUtil _phoneNumberUtil;

    public PhoneNumberValidationService()
    {
        _phoneNumberUtil = PhoneNumberUtil.GetInstance();
    }

    public bool IsValidPhoneNumber(string phoneNumber, string? defaultRegion = null)
    {
        return ValidatePhoneNumber(phoneNumber, defaultRegion).IsValid;
    }

    public string FormatPhoneNumber(string phoneNumber, string? defaultRegion = null)
    {
        var result = ValidatePhoneNumber(phoneNumber, defaultRegion);
        return result.IsValid ? result.E164Format ?? phoneNumber : phoneNumber;
    }

    public PhoneNumberValidationResult ValidatePhoneNumber(string phoneNumber, string? defaultRegion = null)
    {
        var result = new PhoneNumberValidationResult();

        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            result.ErrorMessage = "Phone number cannot be empty";
            return result;
        }

        try
        {
            // Clean the input - remove common formatting characters but preserve + for international format
            var cleanedNumber = CleanPhoneNumber(phoneNumber);
            
            // Parse the phone number
            var parsedNumber = _phoneNumberUtil.Parse(cleanedNumber, defaultRegion);
            
            // Validate the parsed number
            var isValid = _phoneNumberUtil.IsValidNumber(parsedNumber);
            
            if (isValid)
            {
                result.IsValid = true;
                result.CountryCode = parsedNumber.CountryCode.ToString();
                result.RegionCode = _phoneNumberUtil.GetRegionCodeForNumber(parsedNumber);
                result.NumberType = _phoneNumberUtil.GetNumberType(parsedNumber);
                result.E164Format = _phoneNumberUtil.Format(parsedNumber, PhoneNumberFormat.E164);
                result.InternationalFormat = _phoneNumberUtil.Format(parsedNumber, PhoneNumberFormat.INTERNATIONAL);
                result.NationalFormat = _phoneNumberUtil.Format(parsedNumber, PhoneNumberFormat.NATIONAL);
            }
            else
            {
                result.ErrorMessage = "Invalid phone number format";
            }
        }
        catch (NumberParseException ex)
        {
            result.ErrorMessage = GetUserFriendlyErrorMessage(ex.ErrorType);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = $"Error validating phone number: {ex.Message}";
        }

        return result;
    }

    public string GetE164Format(string phoneNumber, string? defaultRegion = null)
    {
        var result = ValidatePhoneNumber(phoneNumber, defaultRegion);
        return result.IsValid ? result.E164Format ?? phoneNumber : phoneNumber;
    }

    public string GetInternationalFormat(string phoneNumber, string? defaultRegion = null)
    {
        var result = ValidatePhoneNumber(phoneNumber, defaultRegion);
        return result.IsValid ? result.InternationalFormat ?? phoneNumber : phoneNumber;
    }

    public string GetNationalFormat(string phoneNumber, string? defaultRegion = null)
    {
        var result = ValidatePhoneNumber(phoneNumber, defaultRegion);
        return result.IsValid ? result.NationalFormat ?? phoneNumber : phoneNumber;
    }

    public bool IsEgyptianPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        var cleanedNumber = CleanPhoneNumber(phoneNumber);
        
        // Check if it's already in international format with +20
        if (cleanedNumber.StartsWith("+20"))
            return true;
            
        // Check if it's a local Egyptian number (starts with 01, 02, 03, etc.)
        if (cleanedNumber.StartsWith("0") && cleanedNumber.Length >= 10 && cleanedNumber.Length <= 11)
        {
            // Egyptian mobile numbers start with 01 (010, 011, 012, 015)
            // Egyptian landline numbers start with 02, 03, etc.
            return cleanedNumber.StartsWith("01") || cleanedNumber.StartsWith("02") || 
                   cleanedNumber.StartsWith("03") || cleanedNumber.StartsWith("04") ||
                   cleanedNumber.StartsWith("05") || cleanedNumber.StartsWith("06") ||
                   cleanedNumber.StartsWith("07") || cleanedNumber.StartsWith("08") ||
                   cleanedNumber.StartsWith("09");
        }
        
        // Check if it's Egyptian number without leading zero (1098021214)
        if (cleanedNumber.Length >= 9 && cleanedNumber.Length <= 10 && 
            (cleanedNumber.StartsWith("1") || cleanedNumber.StartsWith("2") || 
             cleanedNumber.StartsWith("3") || cleanedNumber.StartsWith("4") ||
             cleanedNumber.StartsWith("5") || cleanedNumber.StartsWith("6") ||
             cleanedNumber.StartsWith("7") || cleanedNumber.StartsWith("8") ||
             cleanedNumber.StartsWith("9")))
        {
            return true;
        }
        
        return false;
    }

    private static string CleanPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return phoneNumber;

        // Remove common formatting characters but preserve + for international format
        // Keep only digits, +, and basic formatting characters that libphonenumber can handle
        var cleaned = Regex.Replace(phoneNumber.Trim(), @"[^\d\+\-\(\)\s\.]", "");
        
        return cleaned;
    }

    private static string GetUserFriendlyErrorMessage(ErrorType errorType) => errorType switch
    {
        ErrorType.INVALID_COUNTRY_CODE => "Invalid country code",
        ErrorType.NOT_A_NUMBER => "The input does not contain a valid phone number",
        ErrorType.TOO_SHORT_NSN => "Phone number is too short",
        ErrorType.TOO_SHORT_AFTER_IDD => "Phone number is too short after the international dialing prefix",
        ErrorType.TOO_LONG => "Phone number is too long",
        _ => "Invalid phone number format"
    };
}
