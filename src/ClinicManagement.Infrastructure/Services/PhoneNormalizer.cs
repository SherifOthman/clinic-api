using ClinicManagement.Application.Abstractions.Services;
using PhoneNumbers;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Uses libphonenumber-csharp to extract the national significant number.
///
/// Storage (on write): Parse the full valid E.164 number → GetNationalSignificantNumber()
/// strips the trunk prefix correctly. e.g. "+2001098021259" → "1098021259"
///
/// Search (on read): For partial/incomplete input we cannot use Parse() because
/// libphonenumber refuses to strip the trunk prefix from short numbers.
/// Instead we strip the trunk prefix manually using the region's metadata,
/// then use StartsWith on the NationalNumber column.
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
            // For full valid numbers (E.164 or complete national) — use proper parse
            var region = cleaned.StartsWith("+") ? null : (defaultRegion ?? "EG");
            var parsed = _util.Parse(cleaned, region);

            if (_util.IsValidNumber(parsed))
                return _util.GetNationalSignificantNumber(parsed);
        }
        catch { /* fall through to manual strip */ }

        // For partial/incomplete input (e.g. "010", "0109"):
        // Manually strip the trunk prefix using libphonenumber's metadata.
        // This is the standard approach for prefix search — libphonenumber
        // won't strip trunk prefix from short numbers, so we do it ourselves.
        return StripTrunkPrefix(cleaned, defaultRegion ?? "EG");
    }

    /// <summary>
    /// Strips the national trunk prefix (e.g. "0" in Egypt, UK, Germany)
    /// from a partial number using libphonenumber's region metadata.
    /// Returns the input unchanged if no trunk prefix is found.
    /// </summary>
    private static string StripTrunkPrefix(string digits, string region)
    {
        try
        {
            var metadata = _util.GetMetadataForRegion(region);
            if (metadata == null) return digits;

            var trunkPrefix = metadata.NationalPrefix;
            if (!string.IsNullOrEmpty(trunkPrefix) && digits.StartsWith(trunkPrefix))
                return digits[trunkPrefix.Length..];
        }
        catch { /* ignore */ }

        return digits;
    }
}
