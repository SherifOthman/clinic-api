using ClinicManagement.Domain.Entities;
using ClinicManagement.Application.Common.Models;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IUserManagementService
{
    Task<Result> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default);
    Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IList<string>> GetUserRolesAsync(User user, CancellationToken cancellationToken = default);
    Task<Result> AddToRoleAsync(User user, string role, CancellationToken cancellationToken = default);
    Task<string> GeneratePasswordResetTokenAsync(User user, CancellationToken cancellationToken = default);
    Task<Result> ResetPasswordAsync(User user, string token, string newPassword, CancellationToken cancellationToken = default);
}
