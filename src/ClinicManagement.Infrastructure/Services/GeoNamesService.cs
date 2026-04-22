using System.IO.Compression;
using System.Text;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Downloads and parses GeoNames data files.
///
/// Files (downloaded automatically on first run, cached on disk):
///   countryInfo.txt        - ~270 countries
///   admin1CodesASCII.txt   - ~3,800 states/provinces
///   cities500.zip          - ~200K cities (population > 500)
///   alternateNamesV2.zip   - Arabic name translations
///   cities_processed.tsv   - processed city cache (upload this to server to skip re-download)
/// </summary>
public class GeoNamesService : IGeoNamesService
{
    private readonly HttpClient _http;
    private readonly ILogger<GeoNamesService> _logger;
    private readonly string _baseUrl;
    private readonly string _cacheDir;

    private Dictionary<int, string>? _arabicNamesCache;

    public GeoNamesService(HttpClient http, IHostEnvironment env, ILogger<GeoNamesService> logger, IOptions<GeoNamesOptions> options)
    {
        _http     = http;
        _logger   = logger;
        _baseUrl  = options.Value.BaseUrl.TrimEnd('/');
        var dir   = options.Value.CacheDir;
        _cacheDir = Path.IsPathRooted(dir) ? dir : Path.Combine(env.ContentRootPath, dir);
        Directory.CreateDirectory(_cacheDir);
    }

    // ── Countries ─────────────────────────────────────────────────────────────

    public async Task<List<GeoNamesCountryDump>> GetCountriesAsync(CancellationToken ct = default)
    {
        var text    = await ReadFileAsync("countryInfo.txt", ct);
        var arNames = await GetArabicNamesAsync(ct);
        var result  = new List<GeoNamesCountryDump>();

        foreach (var line in text.Split('\n'))
        {
            if (line.StartsWith('#') || string.IsNullOrWhiteSpace(line)) continue;
            var cols = line.Split('\t');
            if (cols.Length < 17 || !int.TryParse(cols[16].Trim(), out var id)) continue;
            var nameEn = cols[4].Trim();
            result.Add(new GeoNamesCountryDump(id, cols[0].Trim(), nameEn, arNames.GetValueOrDefault(id, nameEn)));
        }

        _logger.LogInformation("Parsed {Count} countries", result.Count);
        return result;
    }

    // ── States ────────────────────────────────────────────────────────────────

    public async Task<List<GeoNamesStateDump>> GetStatesAsync(CancellationToken ct = default)
    {
        var text       = await ReadFileAsync("admin1CodesASCII.txt", ct);
        var arNames    = await GetArabicNamesAsync(ct);
        var countries  = await GetCountriesAsync(ct);
        var countryMap = countries.ToDictionary(c => c.CountryCode, c => c.GeonameId);
        var result     = new List<GeoNamesStateDump>();

        foreach (var line in text.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cols = line.Split('\t');
            if (cols.Length < 4 || !int.TryParse(cols[3].Trim(), out var id)) continue;
            var countryCode = cols[0].Trim().Split('.')[0];
            if (!countryMap.TryGetValue(countryCode, out var countryId)) continue;
            var nameEn = cols[1].Trim();
            result.Add(new GeoNamesStateDump(id, countryId, nameEn, arNames.GetValueOrDefault(id, nameEn)));
        }

        _logger.LogInformation("Parsed {Count} states", result.Count);
        return result;
    }

    // ── Cities ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all cities. Uses cities_processed.tsv cache if available (upload this to server).
    /// Otherwise downloads cities500.zip and processes it.
    /// Deduplicates by (state, name) keeping the highest-population city.
    /// </summary>
    public async Task<List<GeoNamesCityDump>> GetCitiesAsync(CancellationToken ct = default)
    {
        const string Version  = "#v8"; // v8: dedup uses OrdinalIgnoreCase to match SQL Server collation
        var cachePath = Path.Combine(_cacheDir, "cities_processed.tsv");

        // Fast path: cache file exists and is current version
        if (File.Exists(cachePath))
        {
            var lines = await File.ReadAllLinesAsync(cachePath, Encoding.UTF8, ct);
            if (lines.FirstOrDefault()?.StartsWith(Version) == true)
            {
                var cached = ReadCacheLines(lines);
                if (cached.Count > 0)
                {
                    _logger.LogInformation("Loaded {Count:N0} cities from cache", cached.Count);
                    return cached;
                }
            }
            _logger.LogWarning("Cache file is stale — regenerating...");
            File.Delete(cachePath);
        }

        // Slow path: parse cities500.zip
        var cities = await ParseCities500Async(ct);

        // Save cache so next startup is instant (also upload this file to the server)
        await using var writer = new StreamWriter(cachePath, append: false, Encoding.UTF8);
        await writer.WriteLineAsync($"{Version}:{cities.Count}");
        foreach (var c in cities)
            await writer.WriteLineAsync($"{c.GeonameId}\t{c.StateGeonameId}\t{c.NameEn}\t{c.NameAr}");

        _logger.LogInformation("Saved {Count:N0} cities to cache ({MB:F1} MB)",
            cities.Count, new FileInfo(cachePath).Length / 1_048_576.0);
        return cities;
    }

    // ── Private: parse cities500.zip ─────────────────────────────────────────

    private async Task<List<GeoNamesCityDump>> ParseCities500Async(CancellationToken ct)
    {
        var arNames  = await GetArabicNamesAsync(ct);
        var stateMap = await BuildStateMapAsync(ct); // "EG.11" -> stateGeonameId

        var zipPath = Path.Combine(_cacheDir, "cities500.zip");
        if (!File.Exists(zipPath))
        {
            _logger.LogInformation("Downloading cities500.zip (~30 MB)...");
            await File.WriteAllBytesAsync(zipPath, await _http.GetByteArrayAsync(_baseUrl + "/cities500.zip", ct), ct);
        }

        _logger.LogInformation("Parsing cities500.txt...");

        // key = (stateId, nameEn lower) → keep the row with the highest population
        var best = new Dictionary<(int stateId, string name), (int gId, string nameEn, string nameAr, long pop)>();

        using var archive = new ZipArchive(new FileStream(zipPath, FileMode.Open, FileAccess.Read, FileShare.Read));
        var entry  = archive.GetEntry("cities500.txt") ?? archive.Entries.First(e => e.Name.EndsWith(".txt"));
        using var reader = new StreamReader(entry.Open(), Encoding.UTF8);

        string? line;
        while ((line = await reader.ReadLineAsync(ct)) is not null)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cols = line.Split('\t');
            // 0=geonameId  1=name  8=countryCode  10=admin1Code  14=population
            if (cols.Length < 15 || !int.TryParse(cols[0], out var gId)) continue;
            if (!stateMap.TryGetValue($"{cols[8]}.{cols[10]}", out var stateId)) continue;

            var nameEn = cols[1].Trim();
            if (string.IsNullOrWhiteSpace(nameEn)) continue;

            long.TryParse(cols[14], out var pop);
            var key = (stateId, nameEn.ToLowerInvariant());

            if (!best.TryGetValue(key, out var cur) || pop > cur.pop)
                best[key] = (gId, nameEn, arNames.GetValueOrDefault(gId, nameEn), pop);
        }

        var result = best.Select(kvp => new GeoNamesCityDump(kvp.Value.gId, kvp.Key.stateId, kvp.Value.nameEn, kvp.Value.nameAr)).ToList();
        _logger.LogInformation("Parsed {Count:N0} cities (deduplicated by name+state, highest population wins)", result.Count);
        return result;
    }

    // ── Private: helpers ──────────────────────────────────────────────────────

    private async Task<Dictionary<string, int>> BuildStateMapAsync(CancellationToken ct)
    {
        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in (await ReadFileAsync("admin1CodesASCII.txt", ct)).Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cols = line.Split('\t');
            if (cols.Length >= 4 && int.TryParse(cols[3].Trim(), out var gId))
                result[cols[0].Trim()] = gId;
        }
        return result;
    }

    private static List<GeoNamesCityDump> ReadCacheLines(string[] lines)
    {
        var result = new List<GeoNamesCityDump>(lines.Length);
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;
            var cols = line.Split('\t');
            if (cols.Length < 4 || !int.TryParse(cols[0], out var gId) || !int.TryParse(cols[1], out var sId)) continue;
            result.Add(new GeoNamesCityDump(gId, sId, cols[2], cols[3]));
        }
        return result;
    }

    private async Task<Dictionary<int, string>> GetArabicNamesAsync(CancellationToken ct)
    {
        if (_arabicNamesCache is not null) return _arabicNamesCache;

        var tsvPath = Path.Combine(_cacheDir, "ar_names.tsv");

        if (!File.Exists(tsvPath))
        {
            _logger.LogInformation("Downloading alternateNamesV2.zip for Arabic names (~200 MB)...");
            try
            {
                var bytes = await _http.GetByteArrayAsync(_baseUrl + "/alternateNamesV2.zip", ct);
                using var archive = new ZipArchive(new MemoryStream(bytes));
                var entry = archive.GetEntry("alternateNamesV2.txt")
                    ?? archive.Entries.First(e => e.Name.Equals("alternateNamesV2.txt", StringComparison.OrdinalIgnoreCase));

                // Extract only Arabic rows to keep the file small
                var sb = new StringBuilder();
                foreach (var line in (await new StreamReader(entry.Open(), Encoding.UTF8).ReadToEndAsync(ct)).Split('\n'))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var cols = line.Split('\t');
                    if (cols.Length >= 3 && cols[2].Equals("ar", StringComparison.OrdinalIgnoreCase))
                        sb.AppendLine(line);
                }
                await File.WriteAllTextAsync(tsvPath, sb.ToString(), Encoding.UTF8, ct);
                _logger.LogInformation("Saved ar_names.tsv");
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Could not download Arabic names ({Error}). Falling back to English.", ex.Message);
                return _arabicNamesCache = [];
            }
        }

        var result    = new Dictionary<int, string>();
        var preferred = new HashSet<int>();

        foreach (var line in (await File.ReadAllTextAsync(tsvPath, Encoding.UTF8, ct)).Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cols = line.Split('\t');
            if (cols.Length < 4 || !int.TryParse(cols[1].Trim(), out var id)) continue;
            var name         = cols[3].Trim();
            var isPref       = cols.Length > 4 && cols[4] == "1";
            var isColloquial = cols.Length > 6 && cols[6] == "1";
            var isHistoric   = cols.Length > 7 && cols[7] == "1";
            if (isColloquial || isHistoric || name.Length == 0 || name.Length > 150) continue;
            if (name.StartsWith("http", StringComparison.OrdinalIgnoreCase)) continue;
            if (!result.ContainsKey(id) || (isPref && !preferred.Contains(id)))
            {
                result[id] = name;
                if (isPref) preferred.Add(id);
            }
        }

        if (result.Count == 0)
        {
            _logger.LogWarning("ar_names.tsv loaded 0 names — deleting to force re-download.");
            File.Delete(tsvPath);
            return _arabicNamesCache = [];
        }

        _logger.LogInformation("Loaded {Count:N0} Arabic names", result.Count);
        return _arabicNamesCache = result;
    }

    private async Task<string> ReadFileAsync(string fileName, CancellationToken ct)
    {
        var path = Path.Combine(_cacheDir, fileName);
        if (File.Exists(path)) return await File.ReadAllTextAsync(path, Encoding.UTF8, ct);

        _logger.LogInformation("Downloading {File}...", fileName);
        var bytes = await _http.GetByteArrayAsync(_baseUrl + "/" + fileName, ct);
        await File.WriteAllBytesAsync(path, bytes, ct);
        return await File.ReadAllTextAsync(path, Encoding.UTF8, ct);
    }
}
