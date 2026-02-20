using FluentValidation;
using PhoneNumbers;

namespace ClinicManagement.Application.Common.Validators;

public static class PhoneNumberValidator
{
    private static readonly PhoneNumberUtil _phoneUtil = PhoneNumberUtil.GetInstance();

    /// <summary>
    /// Validates phone number in E.164 international format (e.g., +966501234567).
    /// The frontend should send phone numbers with country code.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> MustBeValidPhoneNumber<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(IsValidInternationalPhoneNumber);
    }

    private static bool IsValidInternationalPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return true; // Let Required validator handle empty values

        // Must start with + for international format
        if (!phoneNumber.TrimStart().StartsWith("+"))
            return false;

        try
        {
            // Parse without default region - requires full international format
            var parsedNumber = _phoneUtil.Parse(phoneNumber, null);
            return _phoneUtil.IsValidNumber(parsedNumber);
        }
        catch (NumberParseException)
        {
            return false;
        }
    }
}
