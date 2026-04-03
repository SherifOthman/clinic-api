namespace ClinicManagement.API.Contracts.Auth;

public record LoginRequest(
    string EmailOrUsername,
    string Password
);

public record RegisterRequest(
    string FirstName,
    string LastName,
    string UserName,
    string Email,
    string Gender,
    string Password,
    string PhoneNumber
);

public record ConfirmEmailRequest(
    string Email,
    string Token
);

public record ForgotPasswordRequest(
    string Email
);

public record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword
);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);

public record ResendEmailVerificationRequest(
    string Email
);

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string UserName,
    string PhoneNumber
);

public record RefreshTokenRequest(
    string? RefreshToken
);

public record LogoutRequest(
    string? RefreshToken
);
