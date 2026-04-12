namespace ClinicManagement.Domain.Enums;

public static class BloodTypeExtensions
{
    /// <summary>
    /// Parses a display string (e.g. "A+", "AB-") to a BloodType enum value.
    /// Returns null for unknown or empty input.
    /// </summary>
    public static BloodType? ParseBloodType(string? value) => value switch
    {
        "A+"  => BloodType.APositive,
        "A-"  => BloodType.ANegative,
        "B+"  => BloodType.BPositive,
        "B-"  => BloodType.BNegative,
        "AB+" => BloodType.ABPositive,
        "AB-" => BloodType.ABNegative,
        "O+"  => BloodType.OPositive,
        "O-"  => BloodType.ONegative,
        _     => null,
    };

    /// <summary>
    /// Converts a BloodType enum value to its display string (e.g. "A+").
    /// </summary>
    public static string ToDisplayString(this BloodType bloodType) => bloodType switch
    {
        BloodType.APositive  => "A+",
        BloodType.ANegative  => "A-",
        BloodType.BPositive  => "B+",
        BloodType.BNegative  => "B-",
        BloodType.ABPositive => "AB+",
        BloodType.ABNegative => "AB-",
        BloodType.OPositive  => "O+",
        BloodType.ONegative  => "O-",
        _                    => bloodType.ToString(),
    };
}
