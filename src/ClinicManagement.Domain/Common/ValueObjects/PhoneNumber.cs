using System.Text.RegularExpressions;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Exceptions;

namespace ClinicManagement.Domain.Common.ValueObjects;

/// <summary>
/// PhoneNumber value object - represents a valid phone number with country code
/// Immutable record with automatic value-based equality
/// </summary>
public sealed record PhoneNumber
{
    private static readonly Regex PhoneRegex = new(
        @"^\+[1-9]\d{1,14}$",
        RegexOptions.Compiled);

    public string Value { get; init; }

    private PhoneNumber() { Value = null!; } // EF Core constructor

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidPhoneNumberException("Phone number cannot be empty");

        // Remove common formatting characters
        value = value.Trim()
            .Replace(" ", "")
            .Replace("-", "")
            .Replace("(", "")
            .Replace(")", "")
            .Replace(".", "");

        // Ensure it starts with +
        if (!value.StartsWith("+"))
            throw new InvalidPhoneNumberException(
                "Phone number must start with + and country code");

        if (!PhoneRegex.IsMatch(value))
            throw new InvalidPhoneNumberException(
                "Phone number format is invalid. Must be in E.164 format: +[country code][number]");

        if (value.Length > 16) // E.164 max length is 15 digits + '+'
            throw new InvalidPhoneNumberException(
                "Phone number cannot exceed 15 digits");

        Value = value;
    }

    /// <summary>
    /// Gets the country code (e.g., +1, +44, +966)
    /// </summary>
    public string CountryCode
    {
        get
        {
            // Extract country code (1-3 digits after +)
            var match = Regex.Match(Value, @"^\+(\d{1,3})");
            return match.Success ? $"+{match.Groups[1].Value}" : "+";
        }
    }

    /// <summary>
    /// Gets the number without country code
    /// </summary>
    public string NationalNumber => Value.Substring(CountryCode.Length);

    /// <summary>
    /// Formats the phone number for display
    /// </summary>
    public string Format()
    {
        // Simple formatting: +X (XXX) XXX-XXXX
        if (Value.Length >= 12)
        {
            var countryCode = CountryCode;
            var remaining = NationalNumber;
            
            if (remaining.Length >= 10)
            {
                var areaCode = remaining.Substring(0, 3);
                var firstPart = remaining.Substring(3, 3);
                var secondPart = remaining.Substring(6);
                return $"{countryCode} ({areaCode}) {firstPart}-{secondPart}";
            }
        }

        return Value;
    }

    /// <summary>
    /// Checks if the phone number is from a specific country
    /// </summary>
    public bool IsFromCountry(string countryCode)
    {
        return CountryCode.Equals(countryCode, StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString() => Value;

    // Implicit conversion to string for convenience
    public static implicit operator string(PhoneNumber phone) => phone.Value;

    // Explicit conversion from string (forces validation)
    public static explicit operator PhoneNumber(string value) => new(value);
}
