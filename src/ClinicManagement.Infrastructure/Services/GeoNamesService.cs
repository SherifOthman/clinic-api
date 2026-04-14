using System.IO.Compression;
using System.Text;
using ClinicManagement.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Downloads GeoNames bulk dump files — no API key, no rate limits.
/// Base URL: https://download.geonames.org/export/dump/
///
/// Files used:
///   countryInfo.txt        — all countries (ISO codes, geonameId, English name)
///   admin1CodesASCII.txt   — all ADM1 states (English names + geonameId)
///   cities1000.zip         — cities with pop>1000 or PPLA seats (featureClass=P)
///   alternateNamesV2.zip   — Arabic names for all features
/// </summary>
public class GeoNamesService : IGeoNamesService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeoNamesService> _logger;

    private const string BaseUrl = "https://download.geonames.org/export/dump";

    public GeoNamesService(HttpClient httpClient, ILogger<GeoNamesService> logger)
    {
        _httpClient = httpClient;
        _logger     = logger;
    }

    // ── Countries ─────────────────────────────────────────────────────────────

    public async Task<List<GeoNamesCountryDump>> GetCountriesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Downloading countryInfo.txt from GeoNames dumps...");
        var text = await DownloadTextAsync($"{BaseUrl}/countryInfo.txt", ct);

        // Also download Arabic alternate names for countries
        var arNames = await GetArNamesForFeaturesAsync(ct);

        var countries = new List<GeoNamesCountryDump>();

        foreach (var line in text.Split('\n'))
        {
            if (line.StartsWith('#') || string.IsNullOrWhiteSpace(line)) continue;

            var cols = line.Split('\t');
            if (cols.Length < 17) continue;

            // countryInfo.txt columns: ISO, ISO3, ISO-Numeric, fips, Country, Capital,
            // Area, Population, Continent, tld, CurrencyCode, CurrencyName, Phone,
            // PostalCodeFormat, PostalCodeRegex, Languages, geonameid, neighbours, EquivalentFipsCode
            if (!int.TryParse(cols[16].Trim(), out var geonameId)) continue;

            var countryCode = cols[0].Trim();
            var nameEn      = cols[4].Trim();
            var nameAr      = arNames.GetValueOrDefault(geonameId, nameEn);

            countries.Add(new GeoNamesCountryDump(geonameId, countryCode, nameEn, nameAr));
        }

        _logger.LogInformation("Parsed {Count} countries", countries.Count);
        return countries;
    }

    // ── States ────────────────────────────────────────────────────────────────

    public async Task<List<GeoNamesStateDump>> GetStatesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Downloading admin1CodesASCII.txt from GeoNames dumps...");
        var text = await DownloadTextAsync($"{BaseUrl}/admin1CodesASCII.txt", ct);

        // Build country code → geonameId map from countryInfo
        var countries = await GetCountriesAsync(ct);
        var countryCodeToId = countries.ToDictionary(c => c.CountryCode, c => c.GeonameId);

        var arNames = await GetArNamesForFeaturesAsync(ct);

        var states = new List<GeoNamesStateDump>();

        // admin1CodesASCII.txt: code (CC.ADM1) <tab> name <tab> nameAscii <tab> geonameId
        foreach (var line in text.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cols = line.Split('\t');
            if (cols.Length < 4) continue;

            var code = cols[0].Trim();                          // e.g. "EG.11"
            var nameEn = cols[1].Trim();
            if (!int.TryParse(cols[3].Trim(), out var geonameId)) continue;

            var countryCode = code.Split('.')[0];
            if (!countryCodeToId.TryGetValue(countryCode, out var countryGeonameId)) continue;

            var nameAr = arNames.GetValueOrDefault(geonameId, nameEn);
            states.Add(new GeoNamesStateDump(geonameId, countryGeonameId, nameEn, nameAr));
        }

        _logger.LogInformation("Parsed {Count} states", states.Count);
        return states;
    }

    // ── Cities ────────────────────────────────────────────────────────────────

    public async Task<List<GeoNamesCityDump>> GetCitiesAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Downloading cities1000.zip from GeoNames dumps...");
        var lines = await DownloadZipTextAsync($"{BaseUrl}/cities1000.zip", "cities1000.txt", ct);

        // Build ADM1 code (CC.ADM1) → geonameId map from states
        var states = await GetStatesAsync(ct);

        // We need to map (countryCode + admin1Code) → stateGeonameId
        // admin1CodesASCII uses "CC.ADM1CODE" format
        // cities1000.txt col[8]=countryCode, col[10]=admin1Code
        var admin1Map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var s in states)
        {
            // We need the original admin1 code — re-download to build the map
        }

        // Re-download admin1CodesASCII to get the CC.ADM1 → geonameId mapping
        var admin1Text = await DownloadTextAsync($"{BaseUrl}/admin1CodesASCII.txt", ct);
        foreach (var line in admin1Text.Split('\n'))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var cols = line.Split('\t');
            if (cols.Length < 4) continue;
            if (!int.TryParse(cols[3].Trim(), out var gId)) continue;
            admin1Map[cols[0].Trim()] = gId;  // "EG.11" → geonameId
        }

        var arNames = await GetArNamesForFeaturesAsync(ct);

        // Feature codes to include (matches our original filter)
        var includedFcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { "PPLC", "PPLA", "PPLA2", "PPLA3", "PPLA4" };

        var cities = new List<GeoNamesCityDump>();

        // geoname table columns (tab-separated):
        // 0:geonameId 1:name 2:asciiname 3:alternatenames 4:lat 5:lng
        // 6:featureClass 7:featureCode 8:countryCode 9:cc2
        // 10:admin1Code 11:admin2Code 12:admin3Code 13:admin4Code
        // 14:population 15:elevation 16:dem 17:timezone 18:modificationDate
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

            if (!long.TryParse(cols[14].Trim(), out var population)) population = 0;

            // Apply same filter as before
            var include = includedFcodes.Contains(fcode) ||
                          (fcode == "PPL" && population >= 50_000);
            if (!include) continue;

            // Map to state geonameId
            var admin1Key = $"{countryCode}.{admin1Code}";
            if (!admin1Map.TryGetValue(admin1Key, out var stateGeonameId)) continue;

            var nameAr = arNames.GetValueOrDefault(geonameId, nameEn);
            cities.Add(new GeoNamesCityDump(geonameId, stateGeonameId, nameEn, nameAr));
        }

        _logger.LogInformation("Parsed {Count} cities from cities1000.zip", cities.Count);
        return cities;
    }

    // ── Arabic names ──────────────────────────────────────────────────────────

    /// <summary>
    /// Downloads alternateNamesV2.zip and returns a geonameId → Arabic name map.
    /// Only keeps rows where isolanguage = "ar" and isHistoric is not "1".
    /// Prefers isPreferredName = "1" when multiple Arabic names exist.
    /// </summary>
    private async Task<Dictionary<int, string>> GetArNamesForFeaturesAsync(CancellationToken ct)
    {
        _logger.LogInformation("Downloading alternateNamesV2.zip for Arabic names...");
        var lines = await DownloadZipTextAsync(
            $"{BaseUrl}/alternateNamesV2.zip", "alternateNamesV2.txt", ct);

        // alternateNamesV2.txt columns:
        // 0:alternateNameId 1:geonameId 2:isolanguage 3:alternateName
        // 4:isPreferredName 5:isShortName 6:isColloquial 7:isHistoric 8:from 9:to
        var result   = new Dictionary<int, string>();
        var preferred = new HashSet<int>();

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cols = line.Split('\t');
            if (cols.Length < 4) continue;
            if (!cols[2].Equals("ar", StringComparison.OrdinalIgnoreCase)) continue;
            if (cols.Length > 7 && cols[7] == "1") continue;  // skip historic names

            if (!int.TryParse(cols[1].Trim(), out var geonameId)) continue;

            var name        = cols[3].Trim();
            var isPref      = cols.Length > 4 && cols[4] == "1";
            var isColloquial = cols.Length > 6 && cols[6] == "1";

            if (isColloquial) continue;

            // Only overwrite if this is preferred or we don't have one yet
            if (!result.ContainsKey(geonameId) || (isPref && !preferred.Contains(geonameId)))
            {
                result[geonameId] = name;
                if (isPref) preferred.Add(geonameId);
            }
        }

        _logger.LogInformation("Loaded {Count} Arabic alternate names", result.Count);
        return result;
    }

    // ── Download helpers ──────────────────────────────────────────────────────

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
