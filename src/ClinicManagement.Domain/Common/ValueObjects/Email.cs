using System.Text.RegularExpressions;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Common.Exceptions;

namespace ClinicManagement.Domain.Common.ValueObjects;

/// <summary>
/// Email value object - represents a valid email address
/// Immutable record with automatic value-based equality
/// </summary>
public sealed record Email
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; init; }

    private Email() { Value = null!; } // EF Core constructor

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidEmailException("Email cannot be empty");

        value = value.Trim();

        if (value.Length > 255)
            throw new InvalidEmailException("Email cannot exceed 255 characters");

        if (!EmailRegex.IsMatch(value))
            throw new InvalidEmailException("Email format is invalid");

        Value = value.ToLowerInvariant();
    }

    /// <summary>
    /// Gets the domain part of the email (after @)
    /// </summary>
    public string Domain => Value.Split('@')[1];

    /// <summary>
    /// Gets the local part of the email (before @)
    /// </summary>
    public string LocalPart => Value.Split('@')[0];

    /// <summary>
    /// Checks if the email is from a specific domain
    /// </summary>
    public bool IsFromDomain(string domain)
    {
        return Domain.Equals(domain, StringComparison.OrdinalIgnoreCase);
    }

    public override string ToString() => Value;

    // Implicit conversion to string for convenience
    public static implicit operator string(Email email) => email.Value;

    // Explicit conversion from string (forces validation)
    public static explicit operator Email(string value) => new(value);
}
