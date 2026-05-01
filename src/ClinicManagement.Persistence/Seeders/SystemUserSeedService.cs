using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Persistence.Seeders;

/// <summary>
/// Seeds the four required system users on every environment (local and production).
/// These are not demo data — they are the accounts needed to access and test the system.
///
/// Passwords are read from configuration (SeedOptions) so they can be overridden
/// per environment via environment variables or appsettings.
///
/// Idempotent: skips any user that already exists by email.
/// </summary>
public class SystemUserSeedService
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<SystemUserSeedService> _logger;
    private readonly SeedOptions _opts;

    public SystemUserSeedService(
        UserManager<User> userManager,
        ILogger<SystemUserSeedService> logger,
        IOptions<SeedOptions> opts)
    {
        _userManager = userManager;
        _logger      = logger;
        _opts        = opts.Value;
    }

    public async Task SeedAsync()
    {
        await SeedUserAsync(
            email:    _opts.SuperAdmin.Email,
            username: "superadmin",
            fullName: "System Administrator",
            phone:    "+966500000000",
            gender:   Gender.Male,
            password: _opts.SuperAdmin.Password,
            roles:    [UserRoles.SuperAdmin]);

        await SeedUserAsync(
            email:    _opts.ClinicOwner.Email,
            username: "owner",
            fullName: "Clinic Owner",
            phone:    "+201001234567",
            gender:   Gender.Male,
            password: _opts.ClinicOwner.Password,
            roles:    [UserRoles.ClinicOwner]);

        await SeedUserAsync(
            email:    _opts.Doctor.Email,
            username: "doctor",
            fullName: "Demo Doctor",
            phone:    "+201112345678",
            gender:   Gender.Female,
            password: _opts.Doctor.Password,
            roles:    [UserRoles.Doctor]);

        await SeedUserAsync(
            email:    _opts.Receptionist.Email,
            username: "receptionist",
            fullName: "Demo Receptionist",
            phone:    "+201223456789",
            gender:   Gender.Female,
            password: _opts.Receptionist.Password,
            roles:    [UserRoles.Receptionist]);
    }

    private async Task SeedUserAsync(
        string email, string username, string fullName,
        string phone, Gender gender, string password,
        string[] roles)
    {
        if (await _userManager.FindByEmailAsync(email) is not null)
            return;

        var person = new Person { FullName = fullName, Gender = gender };
        var user   = new User
        {
            UserName       = username,
            Email          = email,
            PhoneNumber    = phone,
            EmailConfirmed = true,
            PersonId       = person.Id,
            Person         = person,
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            _logger.LogError("Failed to seed user {Email}: {Errors}",
                email, string.Join(", ", result.Errors.Select(e => e.Description)));
            return;
        }

        foreach (var role in roles)
            await _userManager.AddToRoleAsync(user, role);

        _logger.LogInformation("System user seeded: {Email} [{Roles}]",
            email, string.Join(", ", roles));
    }
}
