using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<string> roles);
    Task<string> GenerateRefreshTokenAsync(User user, CancellationToken cancellationToken = default);
    public Task RevokeRefreshTokenAsync(string refreshTokens, CancellationToken cancellationToken = default);


}
