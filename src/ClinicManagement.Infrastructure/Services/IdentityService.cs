using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ClinicManagement.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<User> _userManager;

    public IdentityService(
        UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<(bool IsSuccess, string error)> CreateUserAsync(User user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return (true, $"User creation failed:" +
                  $" {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
        return (false, string.Empty);
    }

    public Task<bool> CheckPasswordAsync(User user, string password)
    {
        return _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public Task<User?> GetByUsernameAsync(string username)
    {
        return _userManager.FindByNameAsync(username);
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _userManager.FindByIdAsync(id.ToString());
    }

    public async Task<IEnumerable<string>> GetUserRolesAsync(User user)
    {
        return await _userManager.GetRolesAsync(user);
    }
}
