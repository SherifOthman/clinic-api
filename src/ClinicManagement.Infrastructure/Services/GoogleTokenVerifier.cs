using ClinicManagement.Application.Abstractions.Authentication;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Verifies a Google id_token by calling Google's tokeninfo endpoint.
/// Used by the mobile OAuth flow — the mobile app sends an id_token obtained
/// from the Google Sign-In SDK, and we verify it server-side.
///
/// Google's tokeninfo endpoint validates the signature, expiry, and audience
/// automatically — no manual JWT validation needed.
/// https://developers.google.com/identity/sign-in/web/backend-auth#verify-the-integrity-of-the-id-token
/// </summary>
public class GoogleTokenVerifier : IGoogleTokenVerifier
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GoogleTokenVerifier> _logger;

    public GoogleTokenVerifier(IHttpClientFactory httpClientFactory, ILogger<GoogleTokenVerifier> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger            = logger;
    }

    public async Task<GoogleUserProfile?> VerifyAsync(string idToken, CancellationToken ct = default)
    {
        try
        {
            var client   = _httpClientFactory.CreateClient("Google");
            var url      = $"https://oauth2.googleapis.com/tokeninfo?id_token={Uri.EscapeDataString(idToken)}";
            var response = await client.GetAsync(url, ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Google tokeninfo returned {Status}", response.StatusCode);
                return null;
            }

            var info = await response.Content.ReadFromJsonAsync<GoogleTokenInfoResponse>(ct);

            if (info is null || string.IsNullOrEmpty(info.Email))
            {
                _logger.LogWarning("Google tokeninfo: missing email in response");
                return null;
            }

            return new GoogleUserProfile(
                Email:   info.Email,
                Sub:     info.Sub,
                Name:    info.Name,
                Picture: info.Picture);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify Google id_token");
            return null;
        }
    }

    // ── Internal response model ───────────────────────────────────────────────

    private sealed class GoogleTokenInfoResponse
    {
        [JsonPropertyName("sub")]
        public string? Sub { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("email_verified")]
        public string? EmailVerified { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("picture")]
        public string? Picture { get; set; }
    }
}
