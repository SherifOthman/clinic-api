using System.ComponentModel.DataAnnotations;

namespace ClinicManagement.API.Common.Validation;

/// <summary>
/// Custom validation methods for use with [CustomValidation] attribute
/// </summary>
public static class CustomValidators
{
    public static ValidationResult? MustBeInPast(DateTime date, ValidationContext context)
    {
        return date < DateTime.UtcNow
            ? ValidationResult.Success
            : new ValidationResult("Date must be in the past");
    }

    public static ValidationResult? MustBeInFuture(DateTime date, ValidationContext context)
    {
        return date > DateTime.UtcNow
            ? ValidationResult.Success
            : new ValidationResult("Date must be in the future");
    }

    public static ValidationResult? MustBeInFutureOrNull(DateTime? date, ValidationContext context)
    {
        if (!date.HasValue)
            return ValidationResult.Success;

        return date.Value > DateTime.UtcNow
            ? ValidationResult.Success
            : new ValidationResult("Date must be in the future");
    }

    public static ValidationResult? MustBeInFutureOrToday(DateTime date, ValidationContext context)
    {
        return date.Date >= DateTime.UtcNow.Date
            ? ValidationResult.Success
            : new ValidationResult("Date cannot be in the past");
    }
}
