namespace ClinicManagement.API.Contracts.Auth;

public record LoginRequest(
    string EmailOrUsername,
    string Password
);

public record UpdateProfileRequest(
    string FirstName,
    string LastName,
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
