using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds the SuperAdmin system user.
/// SuperAdmin is not tied to any clinic — manages the whole platform.
/// Credentials are configured via SeedOptions (appsettings / user secrets).
/// </summary>
public class SuperAdminSeedService
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<SuperAdminSeedService> _logger;
    private readonly SeedOptions _options;

    public SuperAdminSeedService(
        UserManager<User> userManager,
        ILogger<SuperAdminSeedService> logger,
        IOptions<SeedOptions> options)
    {
        _userManager = userManager;
        _logger = logger;
        _options = options.Value;
    }

    public async Task SeedAsync()
    {
        var email = _options.SuperAdmin.Email;

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

        var result = await _userManager.CreateAsync(user, _options.SuperAdmin.Password);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to create SuperAdmin: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        await _userManager.AddToRoleAsync(user, "SuperAdmin");
        _logger.LogInformation("SuperAdmin seeded: {Email}", email);
    }
}
