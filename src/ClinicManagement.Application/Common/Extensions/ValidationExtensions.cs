using ClinicManagement.Application.Common.Models;
using FluentValidation;

namespace ClinicManagement.Application.Common.Extensions;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, int> ValidPageNumber<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");
    }

    public static IRuleBuilderOptions<T, int> ValidPageSize<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size cannot exceed 100");
    }

    public static IRuleBuilderOptions<T, int> ValidEntityId<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0)
            .WithMessage("ID must be greater than 0");
    }

    public static IRuleBuilderOptions<T, string> ValidRequiredString<T>(
        this IRuleBuilder<T, string> ruleBuilder, 
        int minLength = 1, 
        int maxLength = 255)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Field is required")
            .Length(minLength, maxLength)
            .WithMessage($"Field must be between {minLength} and {maxLength} characters");
    }

    public static IRuleBuilderOptions<T, string?> ValidOptionalString<T>(
        this IRuleBuilder<T, string?> ruleBuilder, 
        int maxLength = 255)
    {
        return ruleBuilder
            .MaximumLength(maxLength)
            .WithMessage($"Field cannot exceed {maxLength} characters");
    }

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters");
    }

    public static IRuleBuilderOptions<T, string> ValidPhoneNumber<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Phone number is required")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Invalid phone number format");
    }

    public static IRuleBuilderOptions<T, DateTime?> ValidDateRange<T>(
        this IRuleBuilder<T, DateTime?> ruleBuilder,
        DateTime? minDate = null,
        DateTime? maxDate = null)
    {
        IRuleBuilderOptions<T, DateTime?> rule = ruleBuilder.NotNull();

        if (minDate.HasValue)
        {
            rule = rule.GreaterThanOrEqualTo(minDate.Value)
                .WithMessage($"Date must be on or after {minDate.Value:yyyy-MM-dd}");
        }

        if (maxDate.HasValue)
        {
            rule = rule.LessThanOrEqualTo(maxDate.Value)
                .WithMessage($"Date must be on or before {maxDate.Value:yyyy-MM-dd}");
        }

        return rule;
    }
}
