using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<Staff?> GetStaffByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> HasCompletedClinicOnboardingAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<Role>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<List<string>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddUserRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    Task AddUserRoleAsync(Guid userId, Guid roleId, Guid changedBy, string? reason = null, CancellationToken cancellationToken = default);
    Task RemoveUserRoleAsync(Guid userId, Guid roleId, Guid changedBy, string? reason = null, CancellationToken cancellationToken = default);
    Task IncrementFailedLoginAttemptsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task ResetFailedLoginAttemptsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UpdatePasswordAsync(Guid userId, string passwordHash, CancellationToken cancellationToken = default);
}
