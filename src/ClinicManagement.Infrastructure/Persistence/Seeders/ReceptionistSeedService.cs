using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds a demo Receptionist user and links them to the demo clinic.
/// Credentials: receptionist@clinic.com / Receptionist123!
/// </summary>
public class ReceptionistSeedService
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ReceptionistSeedService> _logger;

    public ReceptionistSeedService(
        IApplicationDbContext context,
        UserManager<User> userManager,
        ILogger<ReceptionistSeedService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        const string email = "receptionist@clinic.com";

        var receptionist = await _userManager.FindByEmailAsync(email);
        if (receptionist == null)
        {
            receptionist = new User
            {
                UserName = "receptionist",
                Email = email,
                FirstName = "Emily",
                LastName = "Davis",
                PhoneNumber = "+1234567892",
                EmailConfirmed = true,
                IsMale = false,
            };

            var result = await _userManager.CreateAsync(receptionist, "Receptionist123!");
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to create Receptionist: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }

            await _userManager.AddToRoleAsync(receptionist, "Receptionist");
            _logger.LogInformation("Receptionist user seeded: {Email}", email);
        }

        // Find the demo clinic
        var ownerUser = await _userManager.FindByEmailAsync("owner@clinic.com");
        if (ownerUser == null) return;

        var clinic = await _context.Clinics
            .IgnoreQueryFilters([Domain.Common.Constants.QueryFilterNames.Tenant])
            .FirstOrDefaultAsync(c => c.OwnerUserId == ownerUser.Id);

        if (clinic == null)
        {
            _logger.LogWarning("Demo clinic not found — run ClinicOwnerSeedService first");
            return;
        }

        // Skip if staff record already exists
        var existingStaff = await _context.Staff
            .IgnoreQueryFilters([Domain.Common.Constants.QueryFilterNames.Tenant])
            .FirstOrDefaultAsync(s => s.UserId == receptionist.Id && s.ClinicId == clinic.Id);

        if (existingStaff != null)
        {
            _logger.LogInformation("Receptionist staff record already exists, skipping");
            return;
        }

        var staff = new Staff
        {
            UserId = receptionist.Id,
            ClinicId = clinic.Id,
            IsActive = true,
        };
        _context.Staff.Add(staff);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Receptionist staff record seeded for receptionist@clinic.com");
    }
}
