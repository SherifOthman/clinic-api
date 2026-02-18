using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public class SuperAdminSeedService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly ILogger<SuperAdminSeedService> _logger;

    public SuperAdminSeedService(
        UserManager<User> userManager,
        RoleManager<IdentityRole<int>> roleManager,
        ILogger<SuperAdminSeedService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    public async Task SeedSuperAdminAsync()
    {
        try
        {
            // Check if superadmin already exists
            var existingAdmin = await _userManager.FindByEmailAsync("superadmin@clinic.com");
            if (existingAdmin != null)
            {
                _logger.LogInformation("SuperAdmin user already exists");
                return;
            }

            // Ensure SuperAdmin role exists
            if (!await _roleManager.RoleExistsAsync("SuperAdmin"))
            {
                await _roleManager.CreateAsync(new IdentityRole<int> { Name = "SuperAdmin" });
                _logger.LogInformation("Created SuperAdmin role");
            }

            // Create SuperAdmin user
            var superAdmin = new User
            {
                UserName = "superadmin@clinic.com",
                Email = "superadmin@clinic.com",
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(superAdmin, "SuperAdmin123!");
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                _logger.LogInformation("Created SuperAdmin user with email: superadmin@clinic.com");
            }
            else
            {
                _logger.LogError("Failed to create SuperAdmin user: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding SuperAdmin user");
            throw;
        }
    }
}
