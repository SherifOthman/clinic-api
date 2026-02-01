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
                    NameEn = diseaseSeed.NameEn,
                    NameAr = diseaseSeed.NameAr,
                    DescriptionEn = diseaseSeed.DescriptionEn,
                    DescriptionAr = diseaseSeed.DescriptionAr,
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
                NameEn = "Diabetes",
                NameAr = "السكري",
                DescriptionEn = "A group of metabolic disorders characterized by high blood sugar levels",
                DescriptionAr = "مجموعة من اضطرابات التمثيل الغذائي التي تتميز بارتفاع مستويات السكر في الدم",
                IsActive = true
            },
            new ChronicDisease
            {
                NameEn = "Hypertension",
                NameAr = "ارتفاع ضغط الدم",
                DescriptionEn = "High blood pressure, a condition in which blood pressure is consistently elevated",
                DescriptionAr = "ضغط الدم المرتفع، حالة يكون فيها ضغط الدم مرتفعاً باستمرار",
                IsActive = true
            },
            new ChronicDisease
            {
                NameEn = "Heart Disease",
                NameAr = "أمراض القلب",
                DescriptionEn = "Various conditions that affect the heart and blood vessels",
                DescriptionAr = "حالات مختلفة تؤثر على القلب والأوعية الدموية",
                IsActive = true
            },
            new ChronicDisease
            {
                NameEn = "Asthma",
                NameAr = "الربو",
                DescriptionEn = "A respiratory condition marked by attacks of spasm in the bronchi",
                DescriptionAr = "حالة تنفسية تتميز بنوبات تشنج في القصبات الهوائية",
                IsActive = true
            }
        };

        context.ChronicDiseases.AddRange(chronicDiseases);
        await context.SaveChangesAsync();
    }

    private class ChronicDiseaseSeedData
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = string.Empty;
        public string NameAr { get; set; } = string.Empty;
        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}