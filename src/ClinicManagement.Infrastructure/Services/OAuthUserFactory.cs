using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Creates new user accounts from OAuth provider profiles.
/// Handles username generation and Identity user creation.
/// Shared by all OAuth handlers (Google, and any future providers).
/// </summary>
public class OAuthUserFactory : IOAuthUserFactory
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<OAuthUserFactory> _logger;

    public OAuthUserFactory(UserManager<User> userManager, ILogger<OAuthUserFactory> logger)
    {
        _userManager = userManager;
        _logger      = logger;
    }

    public async Task<User?> CreateAsync(
        string email,
        string fullName,
        string? pictureUrl,
        CancellationToken ct = default)
    {
        var user = new User
        {
            Email           = email,
            UserName        = await GenerateUniqueUsernameAsync(email),
            EmailConfirmed  = true,   // OAuth provider already verified the email
            FullName        = fullName,
            Gender          = Gender.Male,  // default — user can update in profile
            ProfileImageUrl = pictureUrl,
        };

        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to create user from OAuth for {Email}: {Errors}",
                email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return null;
        }

        _logger.LogInformation("Created new user from OAuth: {Email}", email);
        return user;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Derives a username from the email local-part and appends a numeric suffix
    /// until a unique name is found. e.g. "john.doe" → "john.doe2" if taken.
    /// </summary>
    private async Task<string> GenerateUniqueUsernameAsync(string email)
    {
        var baseUsername = email.Split('@')[0];
        var candidate    = baseUsername;
        var suffix       = 1;

        while (await _userManager.FindByNameAsync(candidate) is not null)
            candidate = $"{baseUsername}{suffix++}";

        return candidate;
    }
}
