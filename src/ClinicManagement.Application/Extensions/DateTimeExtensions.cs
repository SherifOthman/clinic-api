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
}
