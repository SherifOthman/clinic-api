using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ClinicManagement.Infrastructure.Data.Seeders;

public static class ClinicSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, IFileSystem fileSystem, ILogger logger)
    {
        if (await context.Clinics.AnyAsync())
        {
            logger.LogInformation("Clinics already exist, skipping clinic seeding");
            return;
        }

        try
        {
            var jsonPath = fileSystem.Combine(fileSystem.GetBaseDirectory(), "SeedData", "clinics.json");
            if (!fileSystem.Exists(jsonPath))
            {
                logger.LogWarning("Clinics seed data file not found at {Path}", jsonPath);
                return;
            }

            var jsonContent = await fileSystem.ReadAllTextAsync(jsonPath);
            var clinicData = JsonSerializer.Deserialize<List<ClinicSeedData>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (clinicData == null || !clinicData.Any())
            {
                logger.LogWarning("No clinic data found in seed file");
                return;
            }

            foreach (var clinicSeed in clinicData)
            {
                var clinic = new Clinic
                {
                    Name = clinicSeed.Name,
                    SubscriptionPlanId = clinicSeed.SubscriptionPlanId,
                    IsActive = clinicSeed.IsActive
                };

                context.Clinics.Add(clinic);
                await context.SaveChangesAsync(); // Save to get the generated ID

                // Add branches
                if (clinicSeed.Branches?.Any() == true)
                {
                    foreach (var branchSeed in clinicSeed.Branches)
                    {
                        var branch = new ClinicBranch
                        {
                            Name = branchSeed.Name,
                            Address = branchSeed.Address,
                            GeoNameId = branchSeed.GeoNameId,
                            ClinicId = clinic.Id,
                            CityName = branchSeed.CityName ?? "Unknown",
                            CountryCode = branchSeed.CountryCode ?? "US", // Use US as default 2-character code
                            Latitude = branchSeed.Latitude ?? 0,
                            Longitude = branchSeed.Longitude ?? 0
                        };

                        context.ClinicBranches.Add(branch);
                        await context.SaveChangesAsync(); // Save to get the generated ID

                        // Add phone numbers
                        if (branchSeed.PhoneNumbers?.Any() == true)
                        {
                            foreach (var phoneSeed in branchSeed.PhoneNumbers)
                            {
                                var phone = new ClinicBranchPhoneNumber
                                {
                                    PhoneNumber = phoneSeed.PhoneNumber,
                                    Label = phoneSeed.Label,
                                    ClinicBranchId = branch.Id
                                };

                                context.ClinicBranchPhoneNumbers.Add(phone);
                            }
                        }
                    }
                }

                logger.LogInformation("Created clinic: {ClinicName} with {BranchCount} branches", 
                    clinic.Name, clinicSeed.Branches?.Count ?? 0);
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Clinic seeding completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding clinics: {Message}", ex.Message);
        }
    }

    private class ClinicSeedData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SubscriptionPlanId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<BranchSeedData>? Branches { get; set; }
    }

    private class BranchSeedData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int GeoNameId { get; set; }
        public string? CityName { get; set; }
        public string? CountryCode { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public List<PhoneNumberSeedData>? PhoneNumbers { get; set; }
    }

    private class PhoneNumberSeedData
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Label { get; set; }
    }
}