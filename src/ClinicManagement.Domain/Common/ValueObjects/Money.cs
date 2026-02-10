using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Exceptions;

namespace ClinicManagement.Domain.Common.ValueObjects;

/// <summary>
/// Money value object - represents an amount with currency
/// Immutable record with automatic value-based equality
/// Supports arithmetic operations with currency validation
/// </summary>
public sealed record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }

    private Money() { Currency = null!; } // EF Core constructor

    public Money(decimal amount, string currency = "USD")
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new InvalidMoneyException("Currency cannot be empty");

        if (currency.Length != 3)
            throw new InvalidMoneyException("Currency must be 3 characters (ISO 4217)");

        Amount = Math.Round(amount, 2); // Round to 2 decimal places
        Currency = currency.ToUpperInvariant();
    }

    /// <summary>
    /// Creates a zero money value
    /// </summary>
    public static Money Zero(string currency = "USD") => new(0, currency);

    /// <summary>
    /// Checks if the amount is zero
    /// </summary>
    public bool IsZero => Amount == 0;

    /// <summary>
    /// Checks if the amount is positive
    /// </summary>
    public bool IsPositive => Amount > 0;

    /// <summary>
    /// Checks if the amount is negative
    /// </summary>
    public bool IsNegative => Amount < 0;

    /// <summary>
    /// Adds two money values (must have same currency)
    /// </summary>
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidMoneyException(
                $"Cannot add different currencies: {Currency} and {other.Currency}");

        return new Money(Amount + other.Amount, Currency);
    }

    /// <summary>
    /// Subtracts two money values (must have same currency)
    /// </summary>
    public Money Subtract(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidMoneyException(
                $"Cannot subtract different currencies: {Currency} and {other.Currency}");

        return new Money(Amount - other.Amount, Currency);
    }

    /// <summary>
    /// Multiplies money by a factor
    /// </summary>
    public Money Multiply(decimal factor)
    {
        return new Money(Amount * factor, Currency);
    }

    /// <summary>
    /// Divides money by a divisor
    /// </summary>
    public Money Divide(decimal divisor)
    {
        if (divisor == 0)
            throw new InvalidMoneyException("Cannot divide by zero");

        return new Money(Amount / divisor, Currency);
    }

    /// <summary>
    /// Calculates percentage of the amount
    /// </summary>
    public Money Percentage(decimal percentage)
    {
        return new Money(Amount * (percentage / 100), Currency);
    }

    /// <summary>
    /// Returns the absolute value
    /// </summary>
    public Money Abs()
    {
        return new Money(Math.Abs(Amount), Currency);
    }

    /// <summary>
    /// Negates the amount
    /// </summary>
    public Money Negate()
    {
        return new Money(-Amount, Currency);
    }

    public override string ToString() => $"{Amount:N2} {Currency}";

    // Operators
    public static Money operator +(Money left, Money right) => left.Add(right);
    public static Money operator -(Money left, Money right) => left.Subtract(right);
    public static Money operator *(Money money, decimal factor) => money.Multiply(factor);
    public static Money operator /(Money money, decimal divisor) => money.Divide(divisor);
    public static Money operator -(Money money) => money.Negate();

    // Comparison operators
    public static bool operator >(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidMoneyException(
                $"Cannot compare different currencies: {left.Currency} and {right.Currency}");

        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidMoneyException(
                $"Cannot compare different currencies: {left.Currency} and {right.Currency}");

        return left.Amount < right.Amount;
    }

    public static bool operator >=(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidMoneyException(
                $"Cannot compare different currencies: {left.Currency} and {right.Currency}");

        return left.Amount >= right.Amount;
    }

    public static bool operator <=(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidMoneyException(
                $"Cannot compare different currencies: {left.Currency} and {right.Currency}");

        return left.Amount <= right.Amount;
    }
}
