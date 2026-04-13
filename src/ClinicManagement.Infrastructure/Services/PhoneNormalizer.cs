using ClinicManagement.Application.Abstractions.Services;
using PhoneNumbers;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Uses libphonenumber-csharp to extract the national significant number.
///
/// Why national significant number?
///   E.164 "+2001098021259" starts with "+200" — StartsWith("010") fails.
///   National number "1098021259" matches StartsWith("109") or StartsWith("1098").
///   The trunk prefix ("0" in Egypt) is stripped by libphonenumber, giving a
///   consistent digits-only value that works across all countries.
/// </summary>
public class PhoneNormalizer : IPhoneNormalizer
{
    private static readonly PhoneNumberUtil _util = PhoneNumberUtil.GetInstance();

    public string? GetNationalNumber(string input, string? defaultRegion = null)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var cleaned = input.Trim()
            .Replace(" ", "")
            .Replace("-", "")
            .Replace("(", "")
            .Replace(")", "");

        try
        {
            // If input starts with + it's already international — no region hint needed
            var region = cleaned.StartsWith("+") ? null : defaultRegion;
            var parsed = _util.Parse(cleaned, region);
            return _util.GetNationalSignificantNumber(parsed);
        }
        catch
        {
            // Unparseable input — return null, caller handles gracefully
            return null;
        }
    }
}
