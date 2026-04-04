using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds the SuperAdmin system user.
/// SuperAdmin is not tied to any clinic — manages the whole platform.
/// Credentials: superadmin@clinic.com / SuperAdmin123!
/// </summary>
public class SuperAdminSeedService
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<SuperAdminSeedService> _logger;

    public SuperAdminSeedService(UserManager<User> userManager, ILogger<SuperAdminSeedService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        const string email = "superadmin@clinic.com";

        if (await _userManager.FindByEmailAsync(email) != null)
        {
            _logger.LogInformation("SuperAdmin already exists, skipping");
            return;
        }

        var user = new User
        {
            UserName = "superadmin",
            Email = email,
            FirstName = "System",
            LastName = "Administrator",
            PhoneNumber = "+966500000000",
            EmailConfirmed = true,
            IsMale = true,
        };

        var result = await _userManager.CreateAsync(user, "SuperAdmin123!");
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to create SuperAdmin: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        await _userManager.AddToRoleAsync(user, "SuperAdmin");
        _logger.LogInformation("SuperAdmin seeded: {Email}", email);
    }
}
