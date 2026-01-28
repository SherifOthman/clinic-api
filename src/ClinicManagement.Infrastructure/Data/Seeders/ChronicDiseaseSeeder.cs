using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ClinicManagement.Infrastructure.Data.Seeders;

public static class ChronicDiseaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, IFileSystem fileSystem, ILogger? logger = null)
    {
        if (await context.ChronicDiseases.AnyAsync())
        {
            logger?.LogInformation("Chronic diseases already exist, skipping seeding");
            return;
        }

        try
        {
            var jsonPath = fileSystem.Combine(fileSystem.GetBaseDirectory(), "SeedData", "chronic-diseases.json");
            if (!fileSystem.Exists(jsonPath))
            {
                logger?.LogWarning("Chronic diseases seed data file not found at {Path}, using default data", jsonPath);
                await SeedDefaultData(context);
                return;
            }

            var jsonContent = await fileSystem.ReadAllTextAsync(jsonPath);
            var diseaseData = JsonSerializer.Deserialize<List<ChronicDiseaseSeedData>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (diseaseData == null || !diseaseData.Any())
            {
                logger?.LogWarning("No chronic disease data found in seed file, using default data");
                await SeedDefaultData(context);
                return;
            }

            foreach (var diseaseSeed in diseaseData)
            {
                var disease = new ChronicDisease
                {
                    Name = diseaseSeed.Name,
                    Description = diseaseSeed.Description,
                    IsActive = diseaseSeed.IsActive
                };

                context.ChronicDiseases.Add(disease);
            }

            await context.SaveChangesAsync();
            logger?.LogInformation("Chronic disease seeding completed with {Count} diseases", diseaseData.Count);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error seeding chronic diseases: {Message}", ex.Message);
            await SeedDefaultData(context);
        }
    }

    private static async Task SeedDefaultData(ApplicationDbContext context)
    {

        var chronicDiseases = new[]
        {
            new ChronicDisease
            {
                Name = "Diabetes",
                Description = "A group of metabolic disorders characterized by high blood sugar levels",
                IsActive = true
            },
            new ChronicDisease
            {
                Name = "Hypertension",
                Description = "High blood pressure, a condition in which blood pressure is consistently elevated",
                IsActive = true
            },
            new ChronicDisease
            {
                Name = "Heart Disease",
                Description = "Various conditions that affect the heart and blood vessels",
                IsActive = true
            },
            new ChronicDisease
            {
                Name = "Asthma",
                Description = "A respiratory condition marked by attacks of spasm in the bronchi",
                IsActive = true
            }
        };

        context.ChronicDiseases.AddRange(chronicDiseases);
        await context.SaveChangesAsync();
    }

    private class ChronicDiseaseSeedData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}