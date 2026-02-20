using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Seeders;

public class SuperAdminSeedService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<SuperAdminSeedService> _logger;

    public SuperAdminSeedService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ILogger<SuperAdminSeedService> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
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
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error seeding users");
            throw;
        }
    }

    private async Task SeedSuperAdminUserAsync()
    {
        var existingAdmin = await _unitOfWork.Users.GetByEmailAsync("superadmin@clinic.com");
        if (existingAdmin != null)
        {
            _logger.LogInformation("SuperAdmin user already exists");
            return;
        }

        await _unitOfWork.BeginTransactionAsync();

        var roles = await _unitOfWork.Users.GetRolesAsync();
        var superAdminRole = roles.FirstOrDefault(r => r.Name == "SuperAdmin");
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
            IsEmailConfirmed = true,
            PasswordHash = _passwordHasher.HashPassword("SuperAdmin123!")
        };

        await _unitOfWork.Users.AddAsync(superAdmin);

        await _unitOfWork.Users.AddUserRoleAsync(superAdmin.Id, superAdminRole.Id);

        await _unitOfWork.CommitTransactionAsync();

        _logger.LogInformation("Created SuperAdmin user with email: superadmin@clinic.com");
    }

    private async Task SeedClinicOwnerUserAsync()
    {
        var existingOwner = await _unitOfWork.Users.GetByEmailAsync("owner@clinic.com");
        if (existingOwner != null)
        {
            _logger.LogInformation("Clinic Owner demo user already exists");
            return;
        }

        await _unitOfWork.BeginTransactionAsync();

        var roles = await _unitOfWork.Users.GetRolesAsync();
        var clinicOwnerRole = roles.FirstOrDefault(r => r.Name == "ClinicOwner");
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
            IsEmailConfirmed = true,
            PasswordHash = _passwordHasher.HashPassword("ClinicOwner123!")
        };

        await _unitOfWork.Users.AddAsync(owner);

        await _unitOfWork.Users.AddUserRoleAsync(owner.Id, clinicOwnerRole.Id);

        await _unitOfWork.CommitTransactionAsync();

        _logger.LogInformation("Created Clinic Owner demo user with email: owner@clinic.com");
    }
}

