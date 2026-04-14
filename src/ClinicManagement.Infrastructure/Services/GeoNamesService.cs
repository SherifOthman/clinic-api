using System.IO.Compression;
using System.Text;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Downloads GeoNames bulk dump files — no API key, no rate limits.
/// Base URL configured via GeoNames:BaseUrl in appsettings.json.
/// Default: https://download.geonames.org/export/dump
/// </summary>
public class GeoNamesService : IGeoNamesService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeoNamesService> _logger;
    private readonly string _baseUrl;

    public GeoNamesService(
        HttpClient httpClient,
        ILogger<GeoNamesService> logger,
        IOptions<GeoNamesOptions> options)
    {
        _httpClient = httpClient;
        _logger     = logger;
        _baseUrl    = options.Value.BaseUrl.TrimEnd('/');
    }

    // ── Countries ─────────────────────────────────────────────────────────────

    public async Task<List<GeoNamesCountryDump>> GetCountriesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Downloading countryInfo.txt...");
        var text    = await DownloadTextAsync(_baseUrl + "/countryInfo.txt", ct);
        var arNames = await GetArNamesAsync(ct);

        var countries = new List<GeoNamesCountryDump>();

        foreach (var line in text.Split('\n'))
        {
            if (line.StartsWith('#') || string.IsNullOrWhiteSpace(line)) continue;
            var cols = line.Split('\t');
            if (cols.Length < 17) continue;
            if (!int.TryParse(cols[16].Trim(), out var geonameId)) continue;

            var countryCode = cols[0].Trim();
            var nameEn      = cols[4].Trim();
            countries.Add(new GeoNamesCountryDump(
                geonameId, countryCode, nameEn,
                arNames.GetValueOrDefault(geonameId, nameEn)));
        }

        _logger.LogInformation("Parsed {Count} countries", countries.Count);
        return countries;
    }

    // ── States ────────────────────────────────────────────────────────────────

    public async Task<List<GeoNamesStateDump>> GetStatesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Downloading admin1CodesASCII.txt...");
        var text    = await DownloadTextAsync(_baseUrl + "/admin1CodesASCII.txt", ct);
        var arNames = await GetArNamesAsync(ct);

        var countries       = await GetCountriesAsync(ct);
        var countryCodeToId = countries.ToDictionary(c => c.CountryCode, c => c.GeonameId);

        var states = new List<GeoNamesStateDump>();

        // Format: CC.ADM1CODE <tab> name <tab> nameAscii <tab> geonameId
        foreach (var line in text.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cols = line.Split('\t');
            if (cols.Length < 4) continue;
            if (!int.TryParse(cols[3].Trim(), out var geonameId)) continue;

            var code        = cols[0].Trim();   // e.g. "EG.11"
            var nameEn      = cols[1].Trim();
            var countryCode = code.Split('.')[0];

            if (!countryCodeToId.TryGetValue(countryCode, out var countryGeonameId)) continue;

            states.Add(new GeoNamesStateDump(
                geonameId, countryGeonameId, nameEn,
                arNames.GetValueOrDefault(geonameId, nameEn)));
        }

        _logger.LogInformation("Parsed {Count} states", states.Count);
        return states;
    }

    // ── Cities ────────────────────────────────────────────────────────────────

    public async Task<List<GeoNamesCityDump>> GetCitiesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Downloading cities1000.zip...");
        var lines   = await DownloadZipTextAsync(_baseUrl + "/cities1000.zip", "cities1000.txt", ct);
        var arNames = await GetArNamesAsync(ct);

        // Build CC.ADM1CODE → stateGeonameId map from admin1CodesASCII
        var admin1Text = await DownloadTextAsync(_baseUrl + "/admin1CodesASCII.txt", ct);
        var admin1Map  = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in admin1Text.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cols = line.Split('\t');
            if (cols.Length < 4) continue;
            if (int.TryParse(cols[3].Trim(), out var gId))
                admin1Map[cols[0].Trim()] = gId;
        }

        // Feature codes to include
        var includedFcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "PPLC", "PPLA", "PPLA2", "PPLA3", "PPLA4" };

        var cities = new List<GeoNamesCityDump>();

        // geoname columns: 0=geonameId 1=name 7=featureCode 8=countryCode 10=admin1Code 14=population
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cols = line.Split('\t');
            if (cols.Length < 15) continue;
            if (!int.TryParse(cols[0].Trim(), out var geonameId)) continue;

            var fcode       = cols[7].Trim();
            var countryCode = cols[8].Trim();
            var admin1Code  = cols[10].Trim();
            var nameEn      = cols[1].Trim();
            long.TryParse(cols[14].Trim(), out var population);

            var include = includedFcodes.Contains(fcode) ||
                          (fcode == "PPL" && population >= 50_000);
            if (!include) continue;

            var admin1Key = countryCode + "." + admin1Code;
            if (!admin1Map.TryGetValue(admin1Key, out var stateGeonameId)) continue;

            cities.Add(new GeoNamesCityDump(
                geonameId, stateGeonameId, nameEn,
                arNames.GetValueOrDefault(geonameId, nameEn)));
        }

        _logger.LogInformation("Parsed {Count} cities from cities1000.zip", cities.Count);
        return cities;
    }

    // ── Arabic names ──────────────────────────────────────────────────────────

    private Dictionary<int, string>? _arNamesCache;

    /// <summary>
    /// Downloads alternateNamesV2.zip once per service instance and returns
    /// a geonameId → Arabic name map. Cached in memory after first download.
    /// </summary>
    private async Task<Dictionary<int, string>> GetArNamesAsync(CancellationToken ct)
    {
        if (_arNamesCache is not null) return _arNamesCache;

        _logger.LogInformation("Downloading alternateNamesV2.zip for Arabic names (~200MB)...");
        var lines = await DownloadZipTextAsync(
            _baseUrl + "/alternateNamesV2.zip", "alternateNamesV2.txt", ct);

        // Columns: 0=alternateNameId 1=geonameId 2=isolanguage 3=alternateName
        //          4=isPreferredName 5=isShortName 6=isColloquial 7=isHistoric
        var result    = new Dictionary<int, string>();
        var preferred = new HashSet<int>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cols = line.Split('\t');
            if (cols.Length < 4) continue;
            if (!cols[2].Equals("ar", StringComparison.OrdinalIgnoreCase)) continue;
            if (cols.Length > 7 && cols[7] == "1") continue;   // skip historic
            if (!int.TryParse(cols[1].Trim(), out var geonameId)) continue;

            var name         = cols[3].Trim();
            var isPref       = cols.Length > 4 && cols[4] == "1";
            var isColloquial = cols.Length > 6 && cols[6] == "1";
            if (isColloquial) continue;

            if (!result.ContainsKey(geonameId) || (isPref && !preferred.Contains(geonameId)))
            {
                result[geonameId] = name;
                if (isPref) preferred.Add(geonameId);
            }
        }

        _logger.LogInformation("Loaded {Count} Arabic alternate names", result.Count);
        _arNamesCache = result;
        return result;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<string> DownloadTextAsync(string url, CancellationToken ct)
    {
        using var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();
        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        return Encoding.UTF8.GetString(bytes);
    }

    private async Task<string[]> DownloadZipTextAsync(string url, string entryName, CancellationToken ct)
    {
        using var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();
        var bytes = await response.Content.ReadAsByteArrayAsync(ct);

        using var ms      = new MemoryStream(bytes);
        using var archive = new ZipArchive(ms, ZipArchiveMode.Read);

        var entry = archive.GetEntry(entryName)
            ?? archive.Entries.FirstOrDefault(e =>
                e.Name.Equals(entryName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Entry '{entryName}' not found in zip");

        using var stream = entry.Open();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var content = await reader.ReadToEndAsync(ct);
        return content.Split('\n');
    }
}
