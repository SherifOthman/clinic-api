using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default);
    Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetUserRolesAsync(User user, CancellationToken cancellationToken = default);
    Task<Result> SetUserRoleAsync(User user, string role, CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
    Task SendConfirmationEmailAsync(User user, CancellationToken cancellationToken = default);
    Task<Result> ConfirmEmailAsync(User user, string token, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(User user, CancellationToken cancellationToken = default);
    Task<Result> ResetPasswordAsync(User user, string token, string newPassword, CancellationToken cancellationToken = default);
    Task<Result> ChangePasswordAsync(User user, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    
    // Email verification methods
    Task<bool> IsEmailConfirmedAsync(User user, CancellationToken cancellationToken = default);
}
