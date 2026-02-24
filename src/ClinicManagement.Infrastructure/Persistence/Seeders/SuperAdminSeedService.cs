using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
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
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var existingAdmin = await unitOfWork.Users.GetByEmailAsync("superadmin@clinic.com");
        if (existingAdmin != null)
        {
            _logger.LogInformation("SuperAdmin user already exists");
            return;
        }

        var roles = await unitOfWork.Users.GetRolesAsync();
        var superAdminRole = roles.FirstOrDefault(r => r.Name == "SuperAdmin");
        if (superAdminRole == null)
        {
            _logger.LogError("SuperAdmin role not found");
            return;
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            var superAdmin = new User
            {
                UserName = "superadmin@clinic.com",
                Email = "superadmin@clinic.com",
                FirstName = "System",
                LastName = "Administrator",
                PhoneNumber = "+966500000000",
                IsEmailConfirmed = true,
                PasswordHash = passwordHasher.HashPassword("SuperAdmin123!")
            };

            await unitOfWork.Users.AddAsync(superAdmin);

            await unitOfWork.Users.AddUserRoleAsync(superAdmin.Id, superAdminRole.Id);

            await unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Created SuperAdmin user with email: superadmin@clinic.com");
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private async Task SeedClinicOwnerUserAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var existingOwner = await unitOfWork.Users.GetByEmailAsync("owner@clinic.com");
        if (existingOwner != null)
        {
            _logger.LogInformation("Clinic Owner demo user already exists");
            return;
        }

        var roles = await unitOfWork.Users.GetRolesAsync();
        var clinicOwnerRole = roles.FirstOrDefault(r => r.Name == "ClinicOwner");
        if (clinicOwnerRole == null)
        {
            _logger.LogError("ClinicOwner role not found");
            return;
        }

        await unitOfWork.BeginTransactionAsync();

        try
        {
            var owner = new User
            {
                UserName = "owner@clinic.com",
                Email = "owner@clinic.com",
                FirstName = "John",
                LastName = "Smith",
                PhoneNumber = "+1234567890",
                IsEmailConfirmed = true,
                PasswordHash = passwordHasher.HashPassword("ClinicOwner123!")
            };

            await unitOfWork.Users.AddAsync(owner);

            await unitOfWork.Users.AddUserRoleAsync(owner.Id, clinicOwnerRole.Id);

            await unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Created Clinic Owner demo user with email: owner@clinic.com");
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
