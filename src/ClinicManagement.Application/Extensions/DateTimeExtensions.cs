namespace ClinicManagement.Application.Extensions;

public static class DateTimeExtensions
{
    public static int CalculateAge(this DateTime dateOfBirth)
    {
        var today = DateTime.UtcNow;
        var age = today.Year - dateOfBirth.Year;
        if (today.DayOfYear < dateOfBirth.DayOfYear)
            age--;
        return age;
    }

    public static DateTime GetDateForAge(int age)
    {
        return DateTime.UtcNow.AddYears(-age);
    }

    public static DateTime GetMaxDateOfBirthForMinAge(int minAge)
    {
        return DateTime.UtcNow.AddYears(-minAge);
    }

    public static DateTime GetMinDateOfBirthForMaxAge(int maxAge)
    {
        return DateTime.UtcNow.AddYears(-maxAge - 1);
    }
}
