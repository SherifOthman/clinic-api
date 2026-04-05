using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds demo patients for the demo clinic.
/// Covers various ages: adult, child, infant, newborn.
/// </summary>
public class PatientSeedService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<PatientSeedService> _logger;

    public PatientSeedService(IApplicationDbContext context, ILogger<PatientSeedService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        // Find the demo clinic
        var clinic = await _context.Clinics
            .IgnoreQueryFilters([Domain.Common.Constants.QueryFilterNames.Tenant])
            .FirstOrDefaultAsync(c => c.Name == "Demo Medical Clinic");

        if (clinic == null)
        {
            _logger.LogWarning("Demo clinic not found — run ClinicOwnerSeedService first");
            return;
        }

        // Skip if patients already seeded
        var existing = await _context.Patients
            .IgnoreQueryFilters([Domain.Common.Constants.QueryFilterNames.Tenant, Domain.Common.Constants.QueryFilterNames.SoftDelete])
            .CountAsync(p => p.ClinicId == clinic.Id);

        if (existing >= 5)
        {
            _logger.LogInformation("Patients already seeded, skipping");
            return;
        }

        var now = DateTime.UtcNow;

        var patients = new[]
        {
            new { Code = "10000001", Name = "Ahmed Hassan Ali",       DOB = new DateTime(1985, 3, 15), Male = true,  Blood = BloodType.APositive,  Phone = "+201012345678", Country = 357994, State = 354500, City = 361360 },
            new { Code = "10000002", Name = "Fatima Mohamed Saad",    DOB = new DateTime(1992, 7, 22), Male = false, Blood = BloodType.BNegative,  Phone = "+201098765432", Country = 357994, State = 354500, City = 361360 },
            new { Code = "10000003", Name = "Omar Khaled Ibrahim",    DOB = new DateTime(2018, 11, 5), Male = true,  Blood = BloodType.OPositive,  Phone = "+201123456789", Country = 357994, State = 354500, City = 361360 },
            new { Code = "10000004", Name = "Nour Ahmed Mostafa",     DOB = new DateTime(2025, 8, 10), Male = false, Blood = BloodType.ABPositive, Phone = "+201234567890", Country = 357994, State = 354500, City = 361360 },
            new { Code = "10000005", Name = "Youssef Tarek Mahmoud",  DOB = new DateTime(1960, 1, 30), Male = true,  Blood = BloodType.ANegative,  Phone = "+201345678901", Country = 357994, State = 354500, City = 361360 },
            new { Code = "10000006", Name = "Layla Sami Hassan",      DOB = new DateTime(2026, 3, 1),  Male = false, Blood = BloodType.ONegative,  Phone = "+201456789012", Country = 357994, State = 354500, City = 361360 },
        };

        foreach (var p in patients)
        {
            // Skip if code already exists
            var exists = await _context.Patients
                .IgnoreQueryFilters([Domain.Common.Constants.QueryFilterNames.Tenant, Domain.Common.Constants.QueryFilterNames.SoftDelete])
                .AnyAsync(x => x.PatientCode == p.Code);
            if (exists) continue;

            var patient = new Patient
            {
                PatientCode      = p.Code,
                ClinicId         = clinic.Id,
                FullName         = p.Name,
                DateOfBirth      = p.DOB,
                IsMale           = p.Male,
                BloodType        = p.Blood,
                CountryGeoNameId = p.Country,
                StateGeoNameId   = p.State,
                CityGeoNameId    = p.City,
                CreatedAt        = now,
            };
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            _context.PatientPhones.Add(new PatientPhone
            {
                PatientId   = patient.Id,
                PhoneNumber = p.Phone,
                IsPrimary   = true,
            });
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} demo patients", patients.Length);
    }
}
