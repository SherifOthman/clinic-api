using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(bool IsSuccess, string error)> CreateUserAsync(User user, string password);
    Task<bool> CheckPasswordAsync(User user, string password);
    Task<IEnumerable<string>> GetUserRolesAsync(User user);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetUserByIdAsync(int id);

}
