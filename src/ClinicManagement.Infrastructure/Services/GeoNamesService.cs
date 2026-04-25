using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Downloads and parses GeoNames data files with minimal memory usage.
///
/// Memory strategy:
///   - Countries / States : small files (~270 / ~3,800 rows) — loaded fully, fine.
///   - Arabic names       : streamed line-by-line from disk, never fully in RAM.
///   - Cities             : streamed via IAsyncEnumerable — one row at a time.
///   - alternateNamesV2   : streamed directly from the zip entry, never MemoryStream.
///
/// Cache files (disk):
///   countryInfo.txt        - countries
///   admin1CodesASCII.txt   - states
///   ar_names.tsv           - Arabic names (extracted from alternateNamesV2.zip)
///   cities_processed.tsv   - processed city cache — upload to server to skip re-download
/// </summary>
public class GeoNamesService : IGeoNamesService
{
    private readonly HttpClient _http;
    private readonly ILogger<GeoNamesService> _logger;
    private readonly string _baseUrl;
    private readonly string _cacheDir;

    public GeoNamesService(
        HttpClient http,
        IHostEnvironment env,
        ILogger<GeoNamesService> logger,
        IOptions<GeoNamesOptions> options)
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
        var arNames = await LoadArabicNamesDictAsync(ct);
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
        var arNames    = await LoadArabicNamesDictAsync(ct);
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

    // ── Cities (streamed) ─────────────────────────────────────────────────────

    /// <summary>
    /// Streams cities one at a time from the cache file or cities500.zip.
    /// Never holds the full list in memory — safe for shared hosting.
    /// </summary>
    public async IAsyncEnumerable<GeoNamesCityDump> StreamCitiesAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        const string Version  = "#v8";
        var cachePath = Path.Combine(_cacheDir, "cities_processed.tsv");

        // Fast path: valid cache file — stream line by line
        if (File.Exists(cachePath))
        {
            var firstLine = true;
            var valid     = false;
            await foreach (var line in ReadLinesAsync(cachePath, ct))
            {
                if (firstLine)
                {
                    firstLine = false;
                    valid     = line.StartsWith(Version);
                    if (!valid) break;
                    continue;
                }
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cols = line.Split('\t');
                if (cols.Length < 4
                    || !int.TryParse(cols[0], out var gId)
                    || !int.TryParse(cols[1], out var sId)) continue;
                yield return new GeoNamesCityDump(gId, sId, cols[2], cols[3]);
            }
            if (valid)
            {
                _logger.LogInformation("Streamed cities from cache file");
                yield break;
            }
            _logger.LogWarning("Cache file stale — regenerating...");
            File.Delete(cachePath);
        }

        // Slow path: parse cities500.zip, write cache, stream results
        _logger.LogInformation("Building city cache from cities500.zip (first run only)...");
        await BuildCityCacheAsync(ct);

        // Now stream from the freshly written cache
        await foreach (var city in StreamCitiesAsync(ct))
            yield return city;
    }

    // ── Private: build city cache ─────────────────────────────────────────────

    /// <summary>
    /// Parses cities500.zip and writes cities_processed.tsv.
    /// Uses a streaming reader for the zip — never loads the full file into RAM.
    /// Arabic names are loaded into a dictionary (~15MB) which is acceptable.
    /// </summary>
    private async Task BuildCityCacheAsync(CancellationToken ct)
    {
        var arNames  = await LoadArabicNamesDictAsync(ct);
        var stateMap = await BuildStateMapAsync(ct);

        var zipPath = Path.Combine(_cacheDir, "cities500.zip");
        if (!File.Exists(zipPath))
        {
            _logger.LogInformation("Downloading cities500.zip (~30 MB)...");
            using var response = await _http.GetAsync(_baseUrl + "/cities500.zip",
                HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();
            await using var fs = File.Create(zipPath);
            await response.Content.CopyToAsync(fs, ct);
        }

        _logger.LogInformation("Parsing cities500.txt and writing cache...");

        // Dedup: keep highest-population city per (state, name)
        var best = new Dictionary<(int stateId, string name),
            (int gId, string nameEn, string nameAr, long pop)>();

        using (var archive = new ZipArchive(
            new FileStream(zipPath, FileMode.Open, FileAccess.Read, FileShare.Read),
            ZipArchiveMode.Read, leaveOpen: false))
        {
            var entry  = archive.GetEntry("cities500.txt")
                      ?? archive.Entries.First(e => e.Name.EndsWith(".txt"));
            using var reader = new StreamReader(entry.Open(), Encoding.UTF8);

            string? line;
            while ((line = await reader.ReadLineAsync(ct)) is not null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cols = line.Split('\t');
                if (cols.Length < 15 || !int.TryParse(cols[0], out var gId)) continue;
                if (!stateMap.TryGetValue($"{cols[8]}.{cols[10]}", out var stateId)) continue;
                var nameEn = cols[1].Trim();
                if (string.IsNullOrWhiteSpace(nameEn)) continue;
                long.TryParse(cols[14], out var pop);
                var key = (stateId, nameEn.ToLowerInvariant());
                if (!best.TryGetValue(key, out var cur) || pop > cur.pop)
                    best[key] = (gId, nameEn, arNames.GetValueOrDefault(gId, nameEn), pop);
            }
        }

        // Free arabic names dict before writing cache
        arNames.Clear();

        var cachePath = Path.Combine(_cacheDir, "cities_processed.tsv");
        await using var writer = new StreamWriter(cachePath, append: false, Encoding.UTF8);
        await writer.WriteLineAsync($"#v8:{best.Count}");
        foreach (var kvp in best)
            await writer.WriteLineAsync(
                $"{kvp.Value.gId}\t{kvp.Key.stateId}\t{kvp.Value.nameEn}\t{kvp.Value.nameAr}");

        _logger.LogInformation("City cache written: {Count:N0} cities", best.Count);

        // Free the dedup dictionary
        best.Clear();
    }

    // ── Private: Arabic names ─────────────────────────────────────────────────

    /// <summary>
    /// Loads Arabic names into a dictionary (~15MB).
    /// Streams the zip entry line-by-line — never loads the 200MB zip into RAM.
    /// </summary>
    private async Task<Dictionary<int, string>> LoadArabicNamesDictAsync(CancellationToken ct)
    {
        var tsvPath = Path.Combine(_cacheDir, "ar_names.tsv");

        if (!File.Exists(tsvPath))
            await DownloadArabicNamesTsvAsync(tsvPath, ct);

        var result    = new Dictionary<int, string>();
        var preferred = new HashSet<int>();

        await foreach (var line in ReadLinesAsync(tsvPath, ct))
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
        }
        else
        {
            _logger.LogInformation("Loaded {Count:N0} Arabic names", result.Count);
        }

        return result;
    }

    private async Task DownloadArabicNamesTsvAsync(string tsvPath, CancellationToken ct)
    {
        _logger.LogInformation("Downloading alternateNamesV2.zip for Arabic names (~200 MB)...");
        try
        {
            // Stream the zip directly — never load into MemoryStream
            using var response = await _http.GetAsync(
                _baseUrl + "/alternateNamesV2.zip",
                HttpCompletionOption.ResponseHeadersRead, ct);
            response.EnsureSuccessStatusCode();

            await using var responseStream = await response.Content.ReadAsStreamAsync(ct);
            using var archive = new ZipArchive(responseStream, ZipArchiveMode.Read, leaveOpen: false);
            var entry = archive.GetEntry("alternateNamesV2.txt")
                     ?? archive.Entries.First(e => e.Name.Equals(
                            "alternateNamesV2.txt", StringComparison.OrdinalIgnoreCase));

            await using var writer = new StreamWriter(tsvPath, append: false, Encoding.UTF8);
            using var reader = new StreamReader(entry.Open(), Encoding.UTF8);

            string? line;
            while ((line = await reader.ReadLineAsync(ct)) is not null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cols = line.Split('\t');
                if (cols.Length >= 3 && cols[2].Equals("ar", StringComparison.OrdinalIgnoreCase))
                    await writer.WriteLineAsync(line);
            }

            _logger.LogInformation("Saved ar_names.tsv");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Could not download Arabic names ({Error}). Falling back to English.", ex.Message);
            // Write empty file so we don't retry on every startup
            await File.WriteAllTextAsync(tsvPath, string.Empty, Encoding.UTF8, ct);
        }
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

    /// <summary>Streams a text file line by line without loading it all into memory.</summary>
    private static async IAsyncEnumerable<string> ReadLinesAsync(
        string path,
        [EnumeratorCancellation] CancellationToken ct)
    {
        using var reader = new StreamReader(path, Encoding.UTF8);
        string? line;
        while ((line = await reader.ReadLineAsync(ct)) is not null)
            yield return line;
    }

    private async Task<string> ReadFileAsync(string fileName, CancellationToken ct)
    {
        var path = Path.Combine(_cacheDir, fileName);
        if (File.Exists(path)) return await File.ReadAllTextAsync(path, Encoding.UTF8, ct);

        _logger.LogInformation("Downloading {File}...", fileName);
        using var response = await _http.GetAsync(_baseUrl + "/" + fileName,
            HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        await File.WriteAllBytesAsync(path, bytes, ct);
        return Encoding.UTF8.GetString(bytes);
    }
}
