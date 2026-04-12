using System.Text.Json;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders;

public class SpecializationSeedService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SpecializationSeedService> _logger;

    public SpecializationSeedService(ApplicationDbContext context, ILogger<SpecializationSeedService> logger)
    {
        _context = context;
        _logger  = logger;
    }

    public async Task SeedSpecializationsAsync()
    {
        if (await _context.Set<Specialization>().AnyAsync()) return;

        var json  = await File.ReadAllTextAsync(ResolvePath("specializations.json"));
        var items = JsonSerializer.Deserialize<SpecializationSeedDto[]>(json, JsonOptions)!;

        _context.Set<Specialization>().AddRange(items.Select(i => new Specialization
        {
            NameEn = i.NameEn, NameAr = i.NameAr,
            DescriptionEn = i.DescriptionEn, DescriptionAr = i.DescriptionAr,
        }));

        await _context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} specializations", items.Length);
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
    private record SpecializationSeedDto(string NameEn, string NameAr, string DescriptionEn, string DescriptionAr);
}
