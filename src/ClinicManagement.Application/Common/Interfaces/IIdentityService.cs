using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(bool IsSuccess,IEnumerable<ErrorItem>? Errors)> CreateUserAsync(User user, string password, CancellationToken cancellationToken = default);
    Task<bool> CheckPasswordAsync(User user, string password, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetUserRolesAsync(User user, CancellationToken cancellationToken = default);
    Task SetUserRoleAsync(User user, string role, CancellationToken cancellationToken = default);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
    Task SendConfirmationEmailAsync(User user, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string Message)> ConfirmEmailAsync(User user, string token, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(User user, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string Message)> ResetPasswordAsync(User user, string token, string newPassword, CancellationToken cancellationToken = default);
    Task<(bool IsSuccess, string Message)> ChangePasswordAsync(User user, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
}
