namespace ClinicManagement.Application.Common.Models;

/// <summary>
/// Shared response for login and token refresh — both return the same shape.
/// </summary>
public record TokenResponseDto(string AccessToken, string? RefreshToken);
