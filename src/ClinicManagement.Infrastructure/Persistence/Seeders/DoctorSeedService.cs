using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds a demo Doctor user and links them to the demo clinic.
/// Credentials are configured via SeedOptions (appsettings / user secrets).
/// </summary>
public class DoctorSeedService
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DoctorSeedService> _logger;
    private readonly SeedOptions _options;

    public DoctorSeedService(
        IApplicationDbContext context,
        UserManager<User> userManager,
        ILogger<DoctorSeedService> logger,
        IOptions<SeedOptions> options)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
        _options = options.Value;
    }

    public async Task SeedAsync()
    {
        var email = _options.Doctor.Email;

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

            var result = await _userManager.CreateAsync(doctor, _options.Doctor.Password);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to create Doctor: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }

            await _userManager.AddToRoleAsync(doctor, "Doctor");
            _logger.LogInformation("Doctor user seeded: {Email}", email);
        }

        // Find the demo clinic
        var ownerUser = await _userManager.FindByEmailAsync(_options.ClinicOwner.Email);
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
