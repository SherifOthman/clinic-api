using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetActiveTokenAsync(string token, DateTimeOffset now, CancellationToken ct = default);
    Task RevokeAllForUserAsync(Guid userId, string ipAddress, DateTimeOffset now, CancellationToken ct = default);
    Task<int> DeleteExpiredAsync(DateTimeOffset now, CancellationToken ct = default);
}
