using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Authentication;

/// <summary>
/// Creates a new user account from an OAuth provider profile.
/// Extracted from GoogleLoginHandler so the same logic is reusable
/// when adding additional OAuth providers (GitHub, Microsoft, etc.)
/// without duplicating username generation or user creation.
/// </summary>
public interface IOAuthUserFactory
{
    /// <summary>
    /// Creates and persists a new user from an OAuth profile.
    /// Generates a unique username derived from the email address.
    /// Returns null if Identity fails to create the user.
    /// </summary>
    Task<User?> CreateAsync(
        string email,
        string fullName,
        string? pictureUrl,
        CancellationToken ct = default);
}
