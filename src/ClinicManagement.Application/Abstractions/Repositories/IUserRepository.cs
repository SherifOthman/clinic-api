using ClinicManagement.Application.Features.Auth.QueryModels;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByIdWithPersonAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailOrUsernameAsync(string emailOrUsername, CancellationToken ct = default);
    Task<bool> AnyByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> AnyByUsernameAsync(string username, CancellationToken ct = default);
    Task<bool> AnyByNormalizedEmailAsync(string normalizedEmail, CancellationToken ct = default);
    Task<bool> AnyByNormalizedUsernameAsync(string normalizedUsername, CancellationToken ct = default);
    Task<List<UserRoleRow>> GetRolesByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<UserSpecializationRow?> GetDoctorSpecializationAsync(Guid userId, CancellationToken ct = default);
    Task<bool> HasClinicAsync(Guid userId, CancellationToken ct = default);
    Task<UserProfileRow?> GetProfileAsync(Guid userId, CancellationToken ct = default);
}
