namespace ClinicManagement.Application.Abstractions.Authentication;

/// <summary>
/// Verifies a Google id_token and returns the user's profile.
/// Implemented in Infrastructure using IHttpClientFactory.
/// Abstracted here so the Application layer has no HTTP dependency.
/// </summary>
public interface IGoogleTokenVerifier
{
    /// <summary>
    /// Verifies the id_token with Google's tokeninfo endpoint.
    /// Returns null if the token is invalid, expired, or the request fails.
    /// </summary>
    Task<GoogleUserProfile?> VerifyAsync(string idToken, CancellationToken ct = default);
}

/// <summary>User profile extracted from a verified Google id_token.</summary>
public record GoogleUserProfile(
    string Email,
    string? Sub,
    string? Name,
    string? Picture
);
