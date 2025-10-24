using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;
public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<IEnumerable<RefreshToken>> GetAllByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task RemoveAllExpiredTokensAsync(CancellationToken cancellationToken = default);
}
