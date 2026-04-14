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
///
/// HOW FILES ARE HANDLED (disk-first strategy):
///   - If the file already exists on disk → read it directly (fast, no internet needed).
///   - If the file is missing → download it from GeoNames, save it to disk, then read it.
///
/// This means files are downloaded only ONCE. After that, every app restart reads from disk.
///
/// FILES USED:
///   countryInfo.txt       — list of all countries (~270 KB)
///   admin1CodesASCII.txt  — list of all states/governorates (~120 KB)
///   cities1000.zip        — cities with population > 1000 (~10 MB zip)
///   ar_names.tsv          — Arabic names extracted from GeoNames (~14 MB)
///
/// WHERE FILES ARE STORED:
///   Configured via appsettings.json → GeoNames:CacheDir (default: SeedData/GeoNames)
/// </summary>
public class GeoNamesService : IGeoNamesService
{
    private readonly HttpClient _http;
    private readonly ILogger<GeoNamesService> _logger;
    private readonly string _baseUrl;  // e.g. https://download.geonames.org/export/dump
    private readonly string _cacheDir; // local folder where files are saved

    // ─────────────────────────────────────────────────────────────────────────
    // ARABIC NAMES — loaded once and cached in memory for the lifetime of the service
    // ─────────────────────────────────────────────────────────────────────────

    // In-memory cache so we don't re-read the file for every method call
    private Dictionary<int, string>? _arabicNamesCache;

    public GeoNamesService(
        HttpClient http,
        IHostEnvironment env,
        ILogger<GeoNamesService> logger,
        IOptions<GeoNamesOptions> options)
    {
        _http    = http;
        _logger  = logger;
        _baseUrl = options.Value.BaseUrl.TrimEnd('/');

        // CacheDir can be an absolute path or relative to the app's root folder
        var cacheDir = options.Value.CacheDir;
        _cacheDir = Path.IsPathRooted(cacheDir)
            ? cacheDir
            : Path.Combine(env.ContentRootPath, cacheDir);

        // Make sure the folder exists before we try to read/write files
        Directory.CreateDirectory(_cacheDir);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PUBLIC METHODS — called by GeoLocationSeedService
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all countries from countryInfo.txt with English and Arabic names.
    ///
    /// countryInfo.txt columns (tab-separated):
    ///   [0]  ISO alpha-2 country code  (e.g. "EG")
    ///   [4]  English name              (e.g. "Egypt")
    ///   [16] GeoNames integer ID       (e.g. 357994)
    /// </summary>
    public async Task<List<GeoNamesCountryDump>> GetCountriesAsync(CancellationToken ct = default)
    {
        var text    = await ReadTextFileAsync("countryInfo.txt", ct);
        var arNames = await GetArabicNamesAsync(ct);
        var result  = new List<GeoNamesCountryDump>();

        foreach (var line in text.Split('\n'))
        {
            // Lines starting with '#' are comments; skip blank lines too
            if (line.StartsWith('#') || string.IsNullOrWhiteSpace(line)) continue;

            var cols = line.Split('\t');
            if (cols.Length < 17) continue;
            if (!int.TryParse(cols[16].Trim(), out var geonameId)) continue;

            var countryCode = cols[0].Trim();
            var nameEn      = cols[4].Trim();

            // Use Arabic name if we have one, otherwise fall back to English
            var nameAr = arNames.GetValueOrDefault(geonameId, nameEn);

            result.Add(new GeoNamesCountryDump(geonameId, countryCode, nameEn, nameAr));
        }

        _logger.LogInformation("Parsed {Count} countries", result.Count);
        return result;
    }

    /// <summary>
    /// Returns all states/governorates from admin1CodesASCII.txt with English and Arabic names.
    ///
    /// admin1CodesASCII.txt columns (tab-separated):
    ///   [0]  "CC.ADM1CODE"  (e.g. "EG.11" — country code dot admin1 code)
    ///   [1]  English name   (e.g. "Kafr el-Sheikh")
    ///   [3]  GeoNames ID    (e.g. 349401)
    ///
    /// We split [0] on '.' to get the country code, then look up the country's GeoNames ID.
    /// </summary>
    public async Task<List<GeoNamesStateDump>> GetStatesAsync(CancellationToken ct = default)
    {
        var text    = await ReadTextFileAsync("admin1CodesASCII.txt", ct);
        var arNames = await GetArabicNamesAsync(ct);

        // Build a map: country code (e.g. "EG") → country GeoNames ID (e.g. 357994)
        var countries      = await GetCountriesAsync(ct);
        var countryCodeMap = countries.ToDictionary(c => c.CountryCode, c => c.GeonameId);

        var result = new List<GeoNamesStateDump>();

        foreach (var line in text.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cols = line.Split('\t');
            if (cols.Length < 4) continue;
            if (!int.TryParse(cols[3].Trim(), out var geonameId)) continue;

            // cols[0] looks like "EG.11" — split on '.' to get "EG"
            var code        = cols[0].Trim();
            var countryCode = code.Split('.')[0];
            var nameEn      = cols[1].Trim();

            // Skip states whose country we don't recognise
            if (!countryCodeMap.TryGetValue(countryCode, out var countryGeonameId)) continue;

            var nameAr = arNames.GetValueOrDefault(geonameId, nameEn);
            result.Add(new GeoNamesStateDump(geonameId, countryGeonameId, nameEn, nameAr));
        }

        _logger.LogInformation("Parsed {Count} states", result.Count);
        return result;
    }

    /// <summary>
    /// Returns cities from cities1000.zip with English and Arabic names.
    ///
    /// cities1000.txt columns (tab-separated, 19 columns total):
    ///   [0]  GeoNames ID     (e.g. 360630)
    ///   [1]  English name    (e.g. "El-Burg")
    ///   [7]  Feature code    (e.g. "PPLA2" — type of place)
    ///   [8]  Country code    (e.g. "EG")
    ///   [10] Admin1 code     (e.g. "11")
    ///   [14] Population      (e.g. 52000)
    ///
    /// WHICH CITIES ARE INCLUDED:
    ///   - PPLC  = capital city
    ///   - PPLA  = state capital
    ///   - PPLA2 = district capital
    ///   - PPLA3 = sub-district capital
    ///   - PPLA4 = minor administrative seat
    ///   - PPL   = populated place with population >= 50,000
    /// </summary>
    public async Task<List<GeoNamesCityDump>> GetCitiesAsync(CancellationToken ct = default)
    {
        var lines   = await ReadZipFileAsync("cities1000.zip", "cities1000.txt", ct);
        var arNames = await GetArabicNamesAsync(ct);

        // Build a map: "CC.ADM1CODE" → state GeoNames ID  (e.g. "EG.11" → 349401)
        // We need this to link each city to its parent state
        var admin1Text = await ReadTextFileAsync("admin1CodesASCII.txt", ct);
        var admin1Map  = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in admin1Text.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cols = line.Split('\t');
            if (cols.Length >= 4 && int.TryParse(cols[3].Trim(), out var gId))
                admin1Map[cols[0].Trim()] = gId;
        }

        // Feature codes that always qualify as a city regardless of population
        var capitalCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "PPLC", "PPLA", "PPLA2", "PPLA3", "PPLA4" };

        var result = new List<GeoNamesCityDump>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cols = line.Split('\t');
            if (cols.Length < 15) continue;
            if (!int.TryParse(cols[0].Trim(), out var geonameId)) continue;

            var nameEn      = cols[1].Trim();
            var fcode       = cols[7].Trim();  // feature code
            var countryCode = cols[8].Trim();
            var admin1Code  = cols[10].Trim();
            long.TryParse(cols[14].Trim(), out var population);

            // Decide whether to include this city
            var isCapitalType  = capitalCodes.Contains(fcode);
            var isLargeEnough  = fcode == "PPL" && population >= 50_000;
            if (!isCapitalType && !isLargeEnough) continue;

            // Look up the parent state using "CC.ADM1CODE" key (e.g. "EG.11")
            var admin1Key = $"{countryCode}.{admin1Code}";
            if (!admin1Map.TryGetValue(admin1Key, out var stateGeonameId)) continue;

            var nameAr = arNames.GetValueOrDefault(geonameId, nameEn);
            result.Add(new GeoNamesCityDump(geonameId, stateGeonameId, nameEn, nameAr));
        }

        _logger.LogInformation("Parsed {Count} cities", result.Count);
        return result;
    }


    /// <summary>
    /// Returns a dictionary of GeoNames ID → Arabic name.
    ///
    /// Source: ar_names.tsv — a pre-filtered file we extracted from alternateNamesV2.zip.
    /// If ar_names.tsv doesn't exist yet, we download the full 200MB zip, extract only
    /// the Arabic rows, and save them as ar_names.tsv (~14MB) for future runs.
    ///
    /// ar_names.tsv columns (same format as alternateNamesV2.txt, tab-separated):
    ///   [1]  GeoNames ID       (e.g. 349401)
    ///   [2]  Language code     (always "ar" in this file)
    ///   [3]  Alternate name    (e.g. "كفر الشيخ")
    ///   [4]  isPreferredName   (1 = preferred, blank = not)
    ///   [6]  isColloquial      (1 = colloquial/slang, skip these)
    ///   [7]  isHistoric        (1 = historic/old name, skip these)
    /// </summary>
    private async Task<Dictionary<int, string>> GetArabicNamesAsync(CancellationToken ct)
    {
        // Return cached result if already loaded
        if (_arabicNamesCache is not null) return _arabicNamesCache;

        string[] lines;
        var tsvPath = Path.Combine(_cacheDir, "ar_names.tsv");

        if (File.Exists(tsvPath))
        {
            // Fast path: ar_names.tsv already exists on disk
            _logger.LogInformation("Reading Arabic names from ar_names.tsv...");
            lines = (await File.ReadAllTextAsync(tsvPath, Encoding.UTF8, ct)).Split('\n');
        }
        else
        {
            // Slow path: download the full 200MB zip, extract Arabic rows, save as tsv
            _logger.LogInformation("ar_names.tsv not found — downloading alternateNamesV2.zip (~200MB)...");
            lines = await ReadZipFileAsync("alternateNamesV2.zip", "alternateNamesV2.txt", ct);

            // Keep only rows where column [2] == "ar"
            _logger.LogInformation("Extracting Arabic rows to ar_names.tsv...");
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var cols = line.Split('\t');
                if (cols.Length >= 3 && cols[2].Equals("ar", StringComparison.OrdinalIgnoreCase))
                    sb.AppendLine(line);
            }

            await File.WriteAllTextAsync(tsvPath, sb.ToString(), Encoding.UTF8, ct);
            _logger.LogInformation("Saved ar_names.tsv ({SizeMB:F1} MB)",
                new FileInfo(tsvPath).Length / 1_048_576.0);
        }

        // Parse the lines into a geonameId → Arabic name dictionary
        var result    = new Dictionary<int, string>();
        var preferred = new HashSet<int>(); // tracks which IDs already have a preferred name

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cols = line.Split('\t');
            if (cols.Length < 4) continue;
            if (!int.TryParse(cols[1].Trim(), out var geonameId)) continue;

            var name         = cols[3].Trim();
            var isPref       = cols.Length > 4 && cols[4] == "1"; // isPreferredName
            var isColloquial = cols.Length > 6 && cols[6] == "1"; // isColloquial
            var isHistoric   = cols.Length > 7 && cols[7] == "1"; // isHistoric

            // Skip unwanted entries
            if (isColloquial || isHistoric) continue;
            if (name.Length == 0 || name.Length > 150) continue;
            if (name.StartsWith("http", StringComparison.OrdinalIgnoreCase)) continue; // skip Wikipedia URLs

            // Store the name; prefer the "isPreferredName" entry if multiple exist
            if (!result.ContainsKey(geonameId) || (isPref && !preferred.Contains(geonameId)))
            {
                result[geonameId] = name;
                if (isPref) preferred.Add(geonameId);
            }
        }

        _logger.LogInformation("Loaded {Count} Arabic names", result.Count);
        _arabicNamesCache = result;
        return result;
    }
    // ─────────────────────────────────────────────────────────────────────────
    // FILE HELPERS — disk-first: read from disk if exists, otherwise download
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Reads a plain text file from disk. If it doesn't exist, downloads it first.
    /// </summary>
    private async Task<string> ReadTextFileAsync(string fileName, CancellationToken ct)
    {
        var path = Path.Combine(_cacheDir, fileName);

        if (!File.Exists(path))
        {
            _logger.LogInformation("Downloading {File}...", fileName);
            var bytes = await _http.GetByteArrayAsync(_baseUrl + "/" + fileName, ct);
            await File.WriteAllBytesAsync(path, bytes, ct);
            _logger.LogInformation("Saved {File} ({SizeKB:F0} KB)", fileName, bytes.Length / 1024.0);
        }

        _logger.LogInformation("Reading {File} from disk...", fileName);
        return await File.ReadAllTextAsync(path, Encoding.UTF8, ct);
    }

    /// <summary>
    /// Reads a specific file from inside a zip archive.
    /// If the zip doesn't exist on disk, downloads it first.
    /// Returns the file content split into lines.
    /// </summary>
    private async Task<string[]> ReadZipFileAsync(string zipName, string entryName, CancellationToken ct)
    {
        var zipPath = Path.Combine(_cacheDir, zipName);

        if (!File.Exists(zipPath))
        {
            _logger.LogInformation("Downloading {Zip}...", zipName);
            var bytes = await _http.GetByteArrayAsync(_baseUrl + "/" + zipName, ct);
            await File.WriteAllBytesAsync(zipPath, bytes, ct);
            _logger.LogInformation("Saved {Zip} ({SizeMB:F1} MB)", zipName, bytes.Length / 1_048_576.0);
        }

        _logger.LogInformation("Reading {Entry} from {Zip}...", entryName, zipName);

        // Open the zip and read the specific file inside it
        var zipBytes = await File.ReadAllBytesAsync(zipPath, ct);
        using var ms      = new MemoryStream(zipBytes);
        using var archive = new ZipArchive(ms, ZipArchiveMode.Read);

        var entry = archive.GetEntry(entryName)
            ?? archive.Entries.FirstOrDefault(e => e.Name.Equals(entryName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"'{entryName}' not found inside '{zipName}'");

        using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
        return (await reader.ReadToEndAsync(ct)).Split('\n');
    }
}
