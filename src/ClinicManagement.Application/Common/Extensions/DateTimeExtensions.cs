namespace ClinicManagement.Application.Common.Extensions;

/// <summary>
/// Extension methods for DateTime operations
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Calculates age from date of birth
    /// </summary>
    public static int CalculateAge(this DateTime dateOfBirth)
    {
        var today = DateTime.UtcNow;
        var age = today.Year - dateOfBirth.Year;
        if (today.DayOfYear < dateOfBirth.DayOfYear)
            age--;
        return age;
    }

    /// <summary>
    /// Gets the date of birth for a specific age
    /// </summary>
    public static DateTime GetDateForAge(int age)
    {
        return DateTime.UtcNow.AddYears(-age);
    }

    /// <summary>
    /// Gets the maximum date of birth for a minimum age
    /// </summary>
    public static DateTime GetMaxDateOfBirthForMinAge(int minAge)
    {
        return DateTime.UtcNow.AddYears(-minAge);
    }

    /// <summary>
    /// Gets the minimum date of birth for a maximum age
    /// </summary>
    public static DateTime GetMinDateOfBirthForMaxAge(int maxAge)
    {
        return DateTime.UtcNow.AddYears(-maxAge - 1);
    }
}
