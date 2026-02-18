using ClinicManagement.Application.Common.Models;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IAuthenticationService
{
    Task<TokenRefreshResult?> RefreshTokenAsync(string? refreshToken, CancellationToken cancellationToken = default);
}
