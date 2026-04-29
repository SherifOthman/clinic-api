namespace ClinicManagement.Application.Abstractions.Services;

/// <summary>
/// Abstracts HttpOnly cookie management for auth tokens.
/// Implemented in Infrastructure; consumed by API controllers.
/// </summary>
public interface ICookieService
{
    void SetAccessTokenCookie(string accessToken, int expiryMinutes);
    void SetRefreshTokenCookie(string refreshToken);
    string? GetAccessTokenFromCookie();
    string? GetRefreshTokenFromCookie();
    void ClearAllAuthCookies();
}
