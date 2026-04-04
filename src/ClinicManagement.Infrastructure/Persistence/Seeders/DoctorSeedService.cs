using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds a demo Doctor user and links them to the demo clinic.
/// Credentials: doctor@clinic.com / Doctor123!
/// </summary>
public class DoctorSeedService
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DoctorSeedService> _logger;

    public DoctorSeedService(
        IApplicationDbContext context,
        UserManager<User> userManager,
        ILogger<DoctorSeedService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        const string email = "doctor@clinic.com";

        var doctor = await _userManager.FindByEmailAsync(email);
        if (doctor == null)
        {
            doctor = new User
            {
                UserName = "doctor",
                Email = email,
                FirstName = "Sarah",
                LastName = "Johnson",
                PhoneNumber = "+1234567891",
                EmailConfirmed = true,
                IsMale = false,
            };

            var result = await _userManager.CreateAsync(doctor, "Doctor123!");
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to create Doctor: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }

            await _userManager.AddToRoleAsync(doctor, "Doctor");
            _logger.LogInformation("Doctor user seeded: {Email}", email);
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
            .FirstOrDefaultAsync(s => s.UserId == doctor.Id && s.ClinicId == clinic.Id);

        if (existingStaff != null)
        {
            _logger.LogInformation("Doctor staff record already exists, skipping");
            return;
        }

        var cardiology = await _context.Specializations.FirstOrDefaultAsync(s => s.NameEn == "Cardiology");

        var staff = new Staff
        {
            UserId = doctor.Id,
            ClinicId = clinic.Id,
            IsActive = true,
        };
        _context.Staff.Add(staff);

        var doctorProfile = new DoctorProfile
        {
            StaffId = staff.Id,
            SpecializationId = cardiology?.Id,
        };
        _context.DoctorProfiles.Add(doctorProfile);

        await _context.SaveChangesAsync();
        _logger.LogInformation("Doctor staff record seeded for doctor@clinic.com");
    }
}
