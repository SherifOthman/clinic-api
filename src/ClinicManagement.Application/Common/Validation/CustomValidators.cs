namespace ClinicManagement.Application.Common.Validation;

/// <summary>
/// Custom validation methods for FluentValidation
/// </summary>
public static class CustomValidators
{
    public static bool MustBeInPast(DateTime date)
    {
        return date < DateTime.UtcNow;
    }

    public static bool MustBeInFuture(DateTime date)
    {
        return date > DateTime.UtcNow;
    }

    public static bool MustBeInFutureOrNull(DateTime? date)
    {
        if (!date.HasValue)
            return true;

        return date.Value > DateTime.UtcNow;
    }

    public static bool MustBeInFutureOrToday(DateTime date)
    {
        return date.Date >= DateTime.UtcNow.Date;
    }
}
