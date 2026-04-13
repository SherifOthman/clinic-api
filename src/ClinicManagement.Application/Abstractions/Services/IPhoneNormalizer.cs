namespace ClinicManagement.Application.Abstractions.Services;

/// <summary>
/// Parses a raw phone string and returns the national significant number
/// (digits only, no country code, no trunk prefix) for indexed search.
/// </summary>
public interface IPhoneNormalizer
{
    /// <summary>
    /// Returns the national significant number for the given input.
    /// e.g. "+2001098021259" → "1098021259"
    ///      "01098021259"   → "1098021259"  (with defaultRegion = "EG")
    /// Returns null if the number cannot be parsed.
    /// </summary>
    string? GetNationalNumber(string input, string? defaultRegion = null);
}
