namespace ClinicManagement.API.Contracts.Auth;

public record LoginRequest(
    string EmailOrUsername,
    string Password
);

public record UpdateProfileRequest(
    string FullName,
    string UserName,
    string PhoneNumber,
    string Gender
);

public record RefreshTokenRequest(
    string? RefreshToken
);

public record LogoutRequest(
    string? RefreshToken
);

/// <summary>
/// Request body for POST /api/auth/oauth/google/mobile.
/// The mobile app obtains this id_token from the Google Sign-In SDK.
/// </summary>
public record GoogleMobileLoginRequest(string IdToken);

/// <summary>Verify email address with a 6-digit OTP.</summary>
public record VerifyEmailOtpRequest(string Email, string Otp);


