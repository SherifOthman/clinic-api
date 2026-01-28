using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data;
using ClinicManagement.Infrastructure.Data.Seeders;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

public interface IDatabaseInitializationService
{
    Task InitializeAsync();
}

public class DatabaseInitializationService : IDatabaseInitializationService
{
    private readonly ApplicationDbContext _context;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<DatabaseInitializationService> _logger;

    public DatabaseInitializationService(
        ApplicationDbContext context,
        RoleManager<IdentityRole<int>> roleManager,
        UserManager<User> userManager,
        IFileSystem fileSystem,
        ILogger<DatabaseInitializationService> logger)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Apply pending migrations
            await _context.Database.MigrateAsync();
            _logger.LogInformation("Database migrations applied successfully");

            // Seed roles first (required for user creation)
            await RoleSeeder.SeedRolesAsync(_roleManager);
            _logger.LogInformation("Roles seeded successfully");

            await SubscriptionPlanSeeder.SeedAsync(_context);
            await ChronicDiseaseSeeder.SeedAsync(_context, _fileSystem, _logger);
            await UserSeeder.SeedAsync(_context, _userManager, _fileSystem, _logger);
            await ClinicSeeder.SeedAsync(_context, _fileSystem, _logger);
            
            // Update users with clinic associations
            await UpdateUserClinicAssociations();
            
            // Seed patients (requires clinics and chronic diseases)
            await PatientSeeder.SeedAsync(_context, _fileSystem, _logger);
            
            _logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database initialization failed: {Message}", ex.Message);
            // Continue anyway - the app might still work
        }
    }

    private async Task UpdateUserClinicAssociations()
    {
        try
        {
            // Get the first two clinics (they should be seeded in order)
            var clinics = await _context.Clinics.OrderBy(c => c.Id).Take(2).ToListAsync();
            if (clinics.Count < 2)
            {
                _logger.LogWarning("Not enough clinics found for user associations");
                return;
            }

            var cityMedicalCenter = clinics[0]; // First clinic
            var familyHealthClinic = clinics[1]; // Second clinic

            // Associate Admin with City Medical Center (for testing purposes)
            var admin = await _userManager.FindByEmailAsync("admin@clinicmanagement.com");
            if (admin != null)
            {
                admin.ClinicId = cityMedicalCenter.Id;
                await _userManager.UpdateAsync(admin);
            }

            // Associate Dr. John Smith (ClinicOwner) with City Medical Center
            var drSmith = await _userManager.FindByEmailAsync("john.smith@example.com");
            if (drSmith != null)
            {
                drSmith.ClinicId = cityMedicalCenter.Id;
                await _userManager.UpdateAsync(drSmith);
            }

            // Associate other doctors with clinics
            var drSarah = await _userManager.FindByEmailAsync("sarah.johnson@example.com");
            if (drSarah != null)
            {
                drSarah.ClinicId = cityMedicalCenter.Id;
                await _userManager.UpdateAsync(drSarah);
            }

            var drAhmed = await _userManager.FindByEmailAsync("ahmed.hassan@example.com");
            if (drAhmed != null)
            {
                drAhmed.ClinicId = familyHealthClinic.Id;
                await _userManager.UpdateAsync(drAhmed);
            }

            var receptionist = await _userManager.FindByEmailAsync("mary.wilson@example.com");
            if (receptionist != null)
            {
                receptionist.ClinicId = cityMedicalCenter.Id;
                await _userManager.UpdateAsync(receptionist);
            }

            _logger.LogInformation("User-clinic associations updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user-clinic associations: {Message}", ex.Message);
        }
    }
}