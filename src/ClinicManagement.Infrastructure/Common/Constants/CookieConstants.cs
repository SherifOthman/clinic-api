namespace ClinicManagement.Infrastructure.Common.Constants;

public static class CookieConstants
{
    public const string AccessToken = "accessToken";
    public const string RefreshToken = "refreshToken";
    public const string UserPreferences = "userPreferences";
    public const string Theme = "theme";

    public static readonly string[] AuthCookies = { AccessToken, RefreshToken };
    public static readonly string[] AllCookies = { AccessToken, RefreshToken, UserPreferences, Theme };
}