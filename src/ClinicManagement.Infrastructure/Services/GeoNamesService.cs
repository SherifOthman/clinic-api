using System.IO.Compression;
using System.Text;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Infrastructure.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Reads GeoNames data files and returns parsed lists of countries, states, and cities.
/// Files are downloaded individually on first use and cached on disk.
/// FILES:
///   countryInfo.txt       - countries (~270 KB)
///   admin1CodesASCII.txt  - states (~120 KB)
///   ar_names.tsv          - Arabic names (~14 MB, pre-extracted from alternateNamesV2.zip)
///   cities_processed.tsv  - pre-processed cities (generated locally and uploaded)
///   cities15000.zip       - fallback source: cities with population > 15,000 (~10 MB)
/// </summary>
public class GeoNamesService : IGeoNamesService
{
    private readonly HttpClient _http;
    private readonly ILogger<GeoNamesService> _logger;
    private readonly string _baseUrl;
    private readonly string _cacheDir;

    private Dictionary<int, string>? _arabicNamesCache;

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

    // ── Public methods ────────────────────────────────────────────────────────

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
            var code   = cols[0].Trim();
            var nameEn = cols[4].Trim();
            result.Add(new GeoNamesCountryDump(id, code, nameEn, arNames.GetValueOrDefault(id, nameEn)));
        }

        _logger.LogInformation("Parsed {Count} countries", result.Count);
        return result;
    }

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

    public async Task<int?> GetExpectedCityCountAsync(CancellationToken ct = default)
    {
        var processedPath = Path.Combine(_cacheDir, "cities_processed.tsv");
        if (!File.Exists(processedPath)) return null;

        using var reader = new StreamReader(processedPath, Encoding.UTF8);
        var firstLine = await reader.ReadLineAsync(ct);
        if (firstLine == null) return null;

        // Support both v5 and v6 headers
        if (firstLine.StartsWith("#v5:") && int.TryParse(firstLine[4..], out var v5count)) return v5count;
        if (firstLine.StartsWith("#v6:") && int.TryParse(firstLine[4..], out var v6count)) return v6count;
        return null;
    }

    public async IAsyncEnumerable<GeoNamesCityDump> StreamCitiesAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        // Accept both v5 and v6 headers when streaming
        var processedPath = Path.Combine(_cacheDir, "cities_processed.tsv");

        if (File.Exists(processedPath))
        {
            string? firstLine;
            using (var check = new StreamReader(processedPath, Encoding.UTF8))
                firstLine = await check.ReadLineAsync(ct);
            if (firstLine == null || (!firstLine.StartsWith("#v5") && !firstLine.StartsWith("#v6")))
            {
                _logger.LogWarning("cities_processed.tsv is stale. Deleting and regenerating...");
                File.Delete(processedPath);
            }
        }

        if (!File.Exists(processedPath))
        {
            _logger.LogInformation("cities_processed.tsv not found - generating from zip first...");
            await GetCitiesAsync(ct);
        }

        if (!File.Exists(processedPath))
        {
            _logger.LogError("cities_processed.tsv could not be generated. City seeding skipped.");
            yield break;
        }

        _logger.LogInformation("Streaming cities from cities_processed.tsv...");
        using var reader = new StreamReader(processedPath, Encoding.UTF8);
        string? line;
        while ((line = await reader.ReadLineAsync(ct)) is not null)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;
            var cols = line.Split('\t');
            if (cols.Length < 4 || !int.TryParse(cols[0], out var gId) || !int.TryParse(cols[1], out var sId)) continue;
            yield return new GeoNamesCityDump(gId, sId, cols[2], cols[3]);
        }
    }

    public async Task<IEnumerable<GeoNamesCityDump>> GetCitiesAsync(CancellationToken ct = default)
    {
        // ── Fast path: pre-processed file ─────────────────────────────────────
        // v6 = cities15000.zip source (population > 15,000), deduplicated by name+state
        // v5 = old allCountries.zip source — treated as stale, will regenerate
        const string versionPrefix = "#v6";
        var processedPath = Path.Combine(_cacheDir, "cities_processed.tsv");
        if (File.Exists(processedPath))
        {
            string? firstLine;
            using (var check = new StreamReader(processedPath, Encoding.UTF8))
                firstLine = await check.ReadLineAsync(ct);

            if (firstLine == null || !firstLine.StartsWith(versionPrefix))
            {
                _logger.LogWarning("cities_processed.tsv is stale (version mismatch - expected v6). Deleting and regenerating...");
                File.Delete(processedPath);
            }
        }

        if (File.Exists(processedPath))
        {
            _logger.LogInformation("Reading cities from cities_processed.tsv...");
            try
            {
                var fileLines = await File.ReadAllLinesAsync(processedPath, Encoding.UTF8, ct);
                var cities    = new List<GeoNamesCityDump>(fileLines.Length);
                foreach (var line in fileLines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;
                    var cols = line.Split('\t');
                    if (cols.Length < 4 || !int.TryParse(cols[0], out var gId) || !int.TryParse(cols[1], out var sId)) continue;
                    cities.Add(new GeoNamesCityDump(gId, sId, cols[2], cols[3]));
                }
                if (cities.Count == 0)
                    throw new InvalidDataException("cities_processed.tsv parsed 0 cities - file is likely corrupted.");
                _logger.LogInformation("Loaded {Count} cities from cities_processed.tsv", cities.Count);
                return cities;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("cities_processed.tsv is corrupted ({Error}). Deleting and regenerating...", ex.Message);
                File.Delete(processedPath);
            }
        }

        // ── Slow path: stream from cities15000.zip ────────────────────────────
        // cities15000.zip contains only cities with population > 15,000 (~26K cities worldwide).
        // This avoids the noise of allCountries.zip (~3.8M features including hamlets/villages).
        //
        // Deduplication strategy: when the same name appears in the same state,
        // keep the row with the HIGHEST POPULATION (most important city wins).
        // This is better than keeping the highest GeonameId (which was arbitrary).
        var arNames    = await GetArabicNamesAsync(ct);
        var admin1Text = await ReadFileAsync("admin1CodesASCII.txt", ct);
        var admin1Map  = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in admin1Text.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cols = line.Split('\t');
            if (cols.Length >= 4 && int.TryParse(cols[3].Trim(), out var gId))
                admin1Map[cols[0].Trim()] = gId;
        }

        var zipFileName = "cities15000.zip";
        var zipPath     = Path.Combine(_cacheDir, zipFileName);

        if (!File.Exists(zipPath))
        {
            _logger.LogInformation("Downloading {File} (~10 MB)...", zipFileName);
            var bytes = await _http.GetByteArrayAsync(_baseUrl + "/" + zipFileName, ct);
            await File.WriteAllBytesAsync(zipPath, bytes, ct);
        }

        _logger.LogInformation("Streaming cities15000.txt from zip...");

        // key = (stateId, nameEn.ToLower()), value = (geonameId, nameEn, nameAr, population)
        var best = new Dictionary<(int stateId, string nameLower), (int gId, string nameEn, string nameAr, long pop)>();

        try
        {
            using var fs      = new FileStream(zipPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var archive = new ZipArchive(fs, ZipArchiveMode.Read);
            var entry  = archive.GetEntry("cities15000.txt")
                ?? archive.Entries.First(e => e.Name.Equals("cities15000.txt", StringComparison.OrdinalIgnoreCase));
            using var reader = new StreamReader(entry.Open(), Encoding.UTF8);

            string? line2;
            while ((line2 = await reader.ReadLineAsync(ct)) is not null)
            {
                if (string.IsNullOrWhiteSpace(line2)) continue;
                var cols = line2.Split('\t');
                // columns: 0=geonameId, 1=name, 8=countryCode, 10=admin1Code, 14=population
                if (cols.Length < 15 || !int.TryParse(cols[0], out var geonameId)) continue;
                if (!admin1Map.TryGetValue($"{cols[8]}.{cols[10]}", out var stateId)) continue;

                var nameEn = cols[1].Trim();
                if (string.IsNullOrWhiteSpace(nameEn)) continue;

                long.TryParse(cols[14], out var population);
                var nameAr = arNames.GetValueOrDefault(geonameId, nameEn);
                var key    = (stateId, nameEn.ToLowerInvariant());

                // Keep the row with the highest population for this name+state combo
                if (!best.TryGetValue(key, out var existing) || population > existing.pop)
                    best[key] = (geonameId, nameEn, nameAr, population);
            }
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException)
        {
            _logger.LogWarning("cities15000.zip appears corrupted ({Error}). Deleting.", ex.Message);
            File.Delete(zipPath);
            throw;
        }

        var result = best
            .Select(kvp => new GeoNamesCityDump(kvp.Value.gId, kvp.Key.stateId, kvp.Value.nameEn, kvp.Value.nameAr))
            .ToList();

        _logger.LogInformation("Parsed {Count} cities (deduplicated by name+state, highest population wins)", result.Count);

        // Write processed file with v6 header
        await using (var writer = new StreamWriter(processedPath, append: false, Encoding.UTF8))
        {
            await writer.WriteLineAsync($"#v6:{result.Count}");
            foreach (var c in result)
                await writer.WriteLineAsync($"{c.GeonameId}\t{c.StateGeonameId}\t{c.NameEn}\t{c.NameAr}");
        }
        _logger.LogInformation("Saved cities_processed.tsv ({MB:F1} MB)", new FileInfo(processedPath).Length / 1_048_576.0);

        return result;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<Dictionary<int, string>> GetArabicNamesAsync(CancellationToken ct)
    {
        if (_arabicNamesCache is not null) return _arabicNamesCache;

        var tsvPath = Path.Combine(_cacheDir, "ar_names.tsv");

        if (!File.Exists(tsvPath))
        {
            _logger.LogInformation("ar_names.tsv not found - downloading alternateNamesV2.zip (~200 MB)...");
            try
            {
                var bytes = await _http.GetByteArrayAsync(_baseUrl + "/alternateNamesV2.zip", ct);
                using var ms      = new MemoryStream(bytes);
                using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
                var entry = archive.GetEntry("alternateNamesV2.txt")
                    ?? archive.Entries.First(e => e.Name.Equals("alternateNamesV2.txt", StringComparison.OrdinalIgnoreCase));
                var rawLines = (await new StreamReader(entry.Open(), Encoding.UTF8).ReadToEndAsync(ct)).Split('\n');

                var sb = new StringBuilder();
                foreach (var line in rawLines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var cols = line.Split('\t');
                    if (cols.Length >= 3 && cols[2].Equals("ar", StringComparison.OrdinalIgnoreCase))
                        sb.AppendLine(line);
                }
                await File.WriteAllTextAsync(tsvPath, sb.ToString(), Encoding.UTF8, ct);
                _logger.LogInformation("Saved ar_names.tsv ({MB:F1} MB)", new FileInfo(tsvPath).Length / 1_048_576.0);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Could not download alternateNamesV2.zip ({Error}). Arabic names will fall back to English.", ex.Message);
                _arabicNamesCache = new Dictionary<int, string>();
                return _arabicNamesCache;
            }
        }

        var lines = (await File.ReadAllTextAsync(tsvPath, Encoding.UTF8, ct)).Split('\n');

        var result    = new Dictionary<int, string>();
        var preferred = new HashSet<int>();
        var lineCount = 0;
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            lineCount++;
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

        _logger.LogInformation("ar_names.tsv: {Lines} lines parsed, {Count} Arabic names loaded",
            lineCount, result.Count);

        if (result.Count == 0)
        {
            _logger.LogWarning("ar_names.tsv loaded 0 names - file is empty or corrupt. Deleting to force re-download.");
            File.Delete(tsvPath);
            _arabicNamesCache = new Dictionary<int, string>();
            return _arabicNamesCache;
        }
        _arabicNamesCache = result;
        return result;
    }

    private async Task<string> ReadFileAsync(string fileName, CancellationToken ct)
    {
        var path = Path.Combine(_cacheDir, fileName);
        if (File.Exists(path))
            return await File.ReadAllTextAsync(path, Encoding.UTF8, ct);

        _logger.LogWarning("File {File} not found at {Path}. Attempting download from {Url}...", fileName, path, _baseUrl);
        try
        {
            var bytes = await _http.GetByteArrayAsync(_baseUrl + "/" + fileName, ct);
            await File.WriteAllBytesAsync(path, bytes, ct);
            return await File.ReadAllTextAsync(path, Encoding.UTF8, ct);
        }
        catch (Exception ex)
        {
            throw new FileNotFoundException(
                $"Required GeoNames file '{fileName}' not found at '{path}' and could not be downloaded from '{_baseUrl}'. " +
                $"Please upload it manually to the server. Error: {ex.Message}", fileName, ex);
        }
    }
}
