using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Common.Enums;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ClinicManagement.Infrastructure.Data.Seeders;

public static class PatientSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, IFileSystem fileSystem, ILogger logger)
    {
        if (await context.Patients.AnyAsync())
        {
            logger.LogInformation("Patients already exist, skipping patient seeding");
            return;
        }

        try
        {
            var jsonPath = fileSystem.Combine(fileSystem.GetBaseDirectory(), "SeedData", "patients.json");
            if (!fileSystem.Exists(jsonPath))
            {
                logger.LogWarning("Patients seed data file not found at {Path}", jsonPath);
                return;
            }

            var jsonContent = await fileSystem.ReadAllTextAsync(jsonPath);
            var patientData = JsonSerializer.Deserialize<List<PatientSeedData>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (patientData == null || !patientData.Any())
            {
                logger.LogWarning("No patient data found in seed file");
                return;
            }

            // Get the clinics to map IDs correctly
            var clinics = await context.Clinics.OrderBy(c => c.Id).ToListAsync();
            if (clinics.Count < 2)
            {
                logger.LogWarning("Not enough clinics found for patient seeding");
                return;
            }

            // Map seed clinic IDs to actual clinic IDs
            var clinicIdMap = new Dictionary<int, int>
            {
                { 1, clinics[0].Id }, // First clinic
                { 2, clinics[1].Id }  // Second clinic
            };

            foreach (var patientSeed in patientData)
            {
                // Map the clinic ID
                if (!clinicIdMap.TryGetValue(patientSeed.ClinicId, out var actualClinicId))
                {
                    logger.LogWarning("Invalid clinic ID {ClinicId} for patient {PatientName}", 
                        patientSeed.ClinicId, $"{patientSeed.FirstName} {patientSeed.LastName}");
                    continue;
                }

                var patient = new Patient
                {
                    FullName = $"{patientSeed.FirstName} {patientSeed.LastName}".Trim(),
                    DateOfBirth = patientSeed.DateOfBirth,
                    Gender = patientSeed.Gender == "Female" ? Gender.Female : Gender.Male,
                    Address = patientSeed.Address,
                    GeoNameId = patientSeed.GeoNameId > 0 ? patientSeed.GeoNameId : null,
                    ClinicId = actualClinicId
                };

                context.Patients.Add(patient);
                await context.SaveChangesAsync(); // Save to get the generated ID

                // Add phone numbers
                if (patientSeed.PhoneNumbers?.Any() == true)
                {
                    foreach (var phoneSeed in patientSeed.PhoneNumbers)
                    {
                        var phone = new PatientPhoneNumber
                        {
                            PhoneNumber = phoneSeed.PhoneNumber,
                            PatientId = patient.Id
                        };

                        context.PatientPhoneNumbers.Add(phone);
                    }
                }

                // Add chronic diseases
                if (patientSeed.ChronicDiseases?.Any() == true)
                {
                    foreach (var diseaseId in patientSeed.ChronicDiseases)
                    {
                        var patientDisease = new PatientChronicDisease
                        {
                            PatientId = patient.Id,
                            ChronicDiseaseId = diseaseId,
                            DiagnosedDate = patientSeed.CreatedAt, // Use patient creation date as diagnosed date
                            IsActive = true
                        };

                        context.PatientChronicDiseases.Add(patientDisease);
                    }
                }

                logger.LogInformation("Created patient: {PatientName} with {PhoneCount} phones and {DiseaseCount} chronic diseases", 
                    patient.FullName, 
                    patientSeed.PhoneNumbers?.Count ?? 0,
                    patientSeed.ChronicDiseases?.Count ?? 0);
            }

            await context.SaveChangesAsync();
            logger.LogInformation("Patient seeding completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error seeding patients: {Message}", ex.Message);
        }
    }

    private class PatientSeedData
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public int GeoNameId { get; set; }
        public int ClinicId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PhoneNumberSeedData>? PhoneNumbers { get; set; }
        public List<int>? ChronicDiseases { get; set; }
    }

    private class PhoneNumberSeedData
    {
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Label { get; set; }
    }
}