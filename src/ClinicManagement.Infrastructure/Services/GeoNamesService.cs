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
///   countryInfo.txt       — countries (~270 KB)
///   admin1CodesASCII.txt  — states (~120 KB)
///   ar_names.tsv          — Arabic names (~14 MB, pre-extracted from alternateNamesV2.zip)
///   cities_processed.tsv  — pre-processed cities (~187 MB, generated locally and uploaded)
///   allCountries.zip      — fallback if cities_processed.tsv is missing (~1.5 GB)
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
            var code  = cols[0].Trim();
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

    public async IAsyncEnumerable<GeoNamesCityDump> StreamCitiesAsync(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        const string expectedHeader = "#v4";
        var processedPath = Path.Combine(_cacheDir, "cities_processed.tsv");

        // Version check
        if (File.Exists(processedPath))
        {
            string? firstLine;
            using (var check = new StreamReader(processedPath, Encoding.UTF8))
                firstLine = await check.ReadLineAsync(ct);
            if (firstLine != expectedHeader)
            {
                _logger.LogWarning("cities_processed.tsv is stale. Deleting and regenerating...");
                File.Delete(processedPath);
            }
        }

        // If file doesn't exist, fall back to GetCitiesAsync to generate it first
        if (!File.Exists(processedPath))
        {
            _logger.LogInformation("cities_processed.tsv not found — generating from zip first...");
            await GetCitiesAsync(ct); // generates and saves the file
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
        // Version header on line 1 guards against stale files generated without
        // the feature-code filter (PPLC/PPLA/PPL only). If the version doesn't
        // match, the file is deleted and regenerated from the zip.
        const string expectedHeader = "#v4";
        var processedPath = Path.Combine(_cacheDir, "cities_processed.tsv");
        if (File.Exists(processedPath))
        {
            string? firstLine;
            using (var check = new StreamReader(processedPath, Encoding.UTF8))
                firstLine = await check.ReadLineAsync(ct);

            if (firstLine != expectedHeader)
            {
                _logger.LogWarning("cities_processed.tsv is stale (missing version header). Deleting and regenerating...");
                File.Delete(processedPath);
            }
        }

        if (File.Exists(processedPath))
        {
            _logger.LogInformation("Reading cities from cities_processed.tsv...");
            try
            {
                var lines  = await File.ReadAllLinesAsync(processedPath, Encoding.UTF8, ct);
                var cities = new List<GeoNamesCityDump>(lines.Length);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;
                    var cols = line.Split('\t');
                    if (cols.Length < 4 || !int.TryParse(cols[0], out var gId) || !int.TryParse(cols[1], out var sId)) continue;
                    cities.Add(new GeoNamesCityDump(gId, sId, cols[2], cols[3]));
                }
                if (cities.Count == 0)
                    throw new InvalidDataException("cities_processed.tsv parsed 0 cities — file is likely corrupted.");
                _logger.LogInformation("Loaded {Count} cities from cities_processed.tsv", cities.Count);
                return cities;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("cities_processed.tsv is corrupted ({Error}). Deleting and regenerating from zip...", ex.Message);
                File.Delete(processedPath);
            }
        }

        // ── Slow path: stream from allCountries.zip ───────────────────────────
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

        var includedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "PPLC", "PPLA", "PPLA2", "PPLA3", "PPLA4", "PPLX" };

        var result  = new List<GeoNamesCityDump>();
        var seenIds = new HashSet<int>();
        var zipPath = Path.Combine(_cacheDir, "allCountries.zip");

        if (!File.Exists(zipPath))
        {
            _logger.LogInformation("Downloading allCountries.zip (~1.5 GB)...");
            var bytes = await _http.GetByteArrayAsync(_baseUrl + "/allCountries.zip", ct);
            await File.WriteAllBytesAsync(zipPath, bytes, ct);
        }

        _logger.LogInformation("Streaming allCountries.txt from zip...");
        try
        {
            using var fs      = new FileStream(zipPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var archive = new ZipArchive(fs, ZipArchiveMode.Read);
            var entry  = archive.GetEntry("allCountries.txt")
                ?? archive.Entries.First(e => e.Name.Equals("allCountries.txt", StringComparison.OrdinalIgnoreCase));
            using var reader = new StreamReader(entry.Open(), Encoding.UTF8);

            string? line2;
            var lineCount = 0;
            while ((line2 = await reader.ReadLineAsync(ct)) is not null)
            {
                if (string.IsNullOrWhiteSpace(line2)) continue;
                var cols = line2.Split('\t');
                if (cols.Length < 15 || !int.TryParse(cols[0], out var geonameId)) continue;
                if (cols[6] != "P") continue;
                if (!includedCodes.Contains(cols[7]) && cols[7] != "PPL") continue;
                if (!admin1Map.TryGetValue($"{cols[8]}.{cols[10]}", out var stateId)) continue;
                var nameEn = cols[1];
                if (seenIds.Add(geonameId))
                    result.Add(new GeoNamesCityDump(geonameId, stateId, nameEn, arNames.GetValueOrDefault(geonameId, nameEn)));
                if (++lineCount % 500_000 == 0)
                    _logger.LogInformation("Cities: scanned {Lines:N0} lines, {Cities:N0} found...", lineCount, result.Count);
            }
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException)
        {
            _logger.LogWarning("allCountries.zip appears corrupted ({Error}). Deleting and will re-download on next startup.", ex.Message);
            File.Delete(zipPath);
            throw;
        }

        _logger.LogInformation("Parsed {Count} cities", result.Count);

        // Deduplicate: keep highest GeonameId when same name appears in same state
        var deduped = result
            .GroupBy(c => (c.StateGeonameId, c.NameEn.ToLowerInvariant()))
            .Select(g => g.OrderByDescending(c => c.GeonameId).First())
            .ToList();

        _logger.LogInformation("After dedup: {Count} cities", deduped.Count);

        // Save for future runs with version header
        var sb = new StringBuilder();
        sb.AppendLine("#v4");
        foreach (var c in deduped)
            sb.AppendLine($"{c.GeonameId}\t{c.StateGeonameId}\t{c.NameEn}\t{c.NameAr}");
        await File.WriteAllTextAsync(processedPath, sb.ToString(), Encoding.UTF8, ct);
        _logger.LogInformation("Saved cities_processed.tsv ({MB:F1} MB)", new FileInfo(processedPath).Length / 1_048_576.0);

        return deduped;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<Dictionary<int, string>> GetArabicNamesAsync(CancellationToken ct)
    {
        if (_arabicNamesCache is not null) return _arabicNamesCache;

        var tsvPath = Path.Combine(_cacheDir, "ar_names.tsv");

        if (!File.Exists(tsvPath))
        {
            _logger.LogInformation("ar_names.tsv not found — downloading alternateNamesV2.zip (~200 MB)...");
            try
            {
                var bytes = await _http.GetByteArrayAsync(_baseUrl + "/alternateNamesV2.zip", ct);
                using var ms      = new MemoryStream(bytes);
                using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
                var entry = archive.GetEntry("alternateNamesV2.txt")
                    ?? archive.Entries.First(e => e.Name.Equals("alternateNamesV2.txt", StringComparison.OrdinalIgnoreCase));
                var rawLines = (await new StreamReader(entry.Open(), Encoding.UTF8).ReadToEndAsync(ct)).Split('\n');

                // Keep only Arabic rows and save
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
                _logger.LogWarning("Could not download alternateNamesV2.zip ({Error}). Arabic names will fall back to English. Upload ar_names.tsv manually to enable Arabic names.", ex.Message);
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

        _logger.LogInformation("ar_names.tsv: {Lines} lines parsed, {Count} Arabic names loaded (file size: {Size:N0} bytes)",
            lineCount, result.Count, new FileInfo(tsvPath).Length);

        if (result.Count == 0)
            _logger.LogWarning("ar_names.tsv loaded 0 names — file may be empty or in wrong format. First line sample: {Sample}",
                lines.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l))?[..Math.Min(200, lines.FirstOrDefault(l => !string.IsNullOrWhiteSpace(l))?.Length ?? 0)] ?? "(empty)");
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
