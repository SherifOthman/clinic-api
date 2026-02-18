using ClinicManagement.Application.Abstractions.Authentication;
using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Abstractions.Storage;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

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
                EmailConfirmed = true,
                PasswordHash = _passwordHasher.HashPassword("SuperAdmin123!"),
                SecurityStamp = Guid.NewGuid().ToString()
            };

            await _unitOfWork.Users.AddAsync(superAdmin);

            await _unitOfWork.Users.AddUserRoleAsync(superAdmin.Id, superAdminRole.Id);

            await _unitOfWork.CommitTransactionAsync();

            _logger.LogInformation("Created SuperAdmin user with email: superadmin@clinic.com");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error seeding SuperAdmin user");
            throw;
        }
    }
}

