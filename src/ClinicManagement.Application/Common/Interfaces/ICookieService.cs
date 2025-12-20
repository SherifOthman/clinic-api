namespace ClinicManagement.Application.Common.Interfaces;

public interface ICookieService
{
    void SetAccessTokenCookie(string accessToken);
    void SetRefreshTokenCookie(string refreshToken);
    string? GetAccessTokenFromCookie();
    string? GetRefreshTokenFromCookie();
    void ClearAuthCookies();
}
