using System.Text.Json;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders;

public class ChronicDiseaseSeedService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ChronicDiseaseSeedService> _logger;

    public ChronicDiseaseSeedService(ApplicationDbContext context, ILogger<ChronicDiseaseSeedService> logger)
    {
        _context = context;
        _logger  = logger;
    }

    public async Task SeedChronicDiseasesAsync()
    {
        if (await _context.Set<ChronicDisease>().AnyAsync()) return;

        var json  = await File.ReadAllTextAsync(ResolvePath("chronic-diseases.json"));
        var items = JsonSerializer.Deserialize<DiseaseSeedDto[]>(json, JsonOptions)!;

        _context.Set<ChronicDisease>().AddRange(items.Select(i => new ChronicDisease
        {
            NameEn = i.NameEn, NameAr = i.NameAr,
            DescriptionEn = i.DescriptionEn, DescriptionAr = i.DescriptionAr,
        }));

        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} chronic diseases", items.Length);
    }

    private static string ResolvePath(string fileName)
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "SeedData", fileName),
            Path.Combine(Directory.GetCurrentDirectory(), "SeedData", fileName),
        };
        return candidates.FirstOrDefault(File.Exists) ?? candidates[0];
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private record DiseaseSeedDto(string NameEn, string NameAr, string DescriptionEn, string DescriptionAr);
}
