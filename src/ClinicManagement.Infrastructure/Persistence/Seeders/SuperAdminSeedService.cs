using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Persistence.Seeders;

public class SuperAdminSeedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SuperAdminSeedService> _logger;

    public SuperAdminSeedService(
        IServiceProvider serviceProvider,
        ILogger<SuperAdminSeedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SeedSuperAdminAsync()
    {
        try
        {
            await SeedSuperAdminUserAsync();
            await SeedClinicOwnerUserAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding users");
            throw;
        }
    }

    private async Task SeedSuperAdminUserAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

        var existingAdmin = await userManager.FindByEmailAsync("superadmin@clinic.com");
        if (existingAdmin != null)
        {
            _logger.LogInformation("SuperAdmin user already exists");
            return;
        }

        var superAdminRole = await roleManager.FindByNameAsync("SuperAdmin");
        if (superAdminRole == null)
        {
            _logger.LogError("SuperAdmin role not found");
            return;
        }

        var superAdmin = new User
        {
            UserName = "superadmin@clinic.com",
            Email = "superadmin@clinic.com",
            FirstName = "System",
            LastName = "Administrator",
            PhoneNumber = "+966500000000",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(superAdmin, "SuperAdmin123!");
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create SuperAdmin user: {Errors}", errors);
            return;
        }

        await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");

        _logger.LogInformation("Created SuperAdmin user with email: superadmin@clinic.com");
    }

    private async Task SeedClinicOwnerUserAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

        var existingOwner = await userManager.FindByEmailAsync("owner@clinic.com");
        if (existingOwner != null)
        {
            _logger.LogInformation("Clinic Owner demo user already exists");
            return;
        }

        var clinicOwnerRole = await roleManager.FindByNameAsync("ClinicOwner");
        if (clinicOwnerRole == null)
        {
            _logger.LogError("ClinicOwner role not found");
            return;
        }

        var owner = new User
        {
            UserName = "owner@clinic.com",
            Email = "owner@clinic.com",
            FirstName = "John",
            LastName = "Smith",
            PhoneNumber = "+1234567890",
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(owner, "ClinicOwner123!");
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create Clinic Owner user: {Errors}", errors);
            return;
        }

        await userManager.AddToRoleAsync(owner, "ClinicOwner");

        _logger.LogInformation("Created Clinic Owner demo user with email: owner@clinic.com");
    }
}
