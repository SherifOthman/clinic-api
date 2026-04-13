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
            var region = cleaned.StartsWith("+") ? null : (defaultRegion ?? "EG");

            // Use ParseHelper which does NOT validate — it just parses the structure.
            // This allows partial/incomplete numbers like "010" to still yield
            // a national significant number for prefix search.
            var parsed = _util.ParseAndKeepRawInput(cleaned, region);
            return _util.GetNationalSignificantNumber(parsed);
        }
        catch
        {
            return null;
        }
    }
}
