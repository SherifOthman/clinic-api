using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Persistence.Seeders;

public class RoleSeedService
{
    private readonly RoleManager<Role> _roleManager;
    private readonly ILogger<RoleSeedService> _logger;

    public RoleSeedService(
        RoleManager<Role> roleManager,
        ILogger<RoleSeedService> logger)
    {
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedRolesAsync()
    {
        var roles = new[]
        {
            UserRoles.SuperAdmin,
            UserRoles.ClinicOwner,
            UserRoles.Doctor,
            UserRoles.Receptionist
        };

        foreach (var roleName in roles)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                var role = new Role { Name = roleName };
                var result = await _roleManager.CreateAsync(role);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("Created role: {RoleName}", roleName);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create role {RoleName}: {Errors}", roleName, errors);
                }
            }
        }
    }
}
