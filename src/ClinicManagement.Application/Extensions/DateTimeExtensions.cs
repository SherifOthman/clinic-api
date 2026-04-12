namespace ClinicManagement.Application.Extensions;

public static class DateTimeExtensions
{
    public static int CalculateAge(this DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age   = today.Year - dateOfBirth.Year;
        if (today.DayOfYear < dateOfBirth.DayOfYear)
            age--;
        return age;
    }
}
