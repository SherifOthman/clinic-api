namespace ClinicManagement.Application.Common.Models;

/// <summary>
/// <summary>
/// Shared response for login and token refresh.
/// Web clients: both fields are null (tokens are in HttpOnly cookies).
/// Mobile clients: both fields are populated.
/// </summary>
public record TokenResponseDto(string? AccessToken, string? RefreshToken);
