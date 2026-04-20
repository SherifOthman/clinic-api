#!/usr/bin/env dotnet-script
// Run with: dotnet script GenerateGeoSeedSql.cs
// Generates SQL INSERT scripts for GeoCountries and GeoStates
// from the local GeoNames files. Run this locally, then execute
// the SQL on the server via the DB manager.

using System.IO;
using System.Text;

var cacheDir = Path.Combine(AppContext.BaseDirectory, "..", "src", "ClinicManagement.API", "SeedData", "GeoNames");
Console.WriteLine($"Reading from: {Path.GetFullPath(cacheDir)}");

// ── Arabic names ──────────────────────────────────────────────────────────────
var arNames = new Dictionary<int, string>();
var arPath  = Path.Combine(cacheDir, "ar_names.tsv");
if (File.Exists(arPath))
{
    foreach (var line in File.ReadLines(arPath))
    {
        if (string.IsNullOrWhiteSpace(line)) continue;
        var cols = line.Split('\t');
        if (cols.Length < 4 || !int.TryParse(cols[1].Trim(), out var id)) continue;
        var name = cols[3].Trim();
        var isPref = cols.Length > 4 && cols[4] == "1";
        if (name.Length == 0 || name.Length > 150) continue;
        if (!arNames.ContainsKey(id) || isPref) arNames[id] = name;
    }
    Console.WriteLine($"Loaded {arNames.Count} Arabic names");
}

// ── Countries ─────────────────────────────────────────────────────────────────
var countrySql = new StringBuilder();
countrySql.AppendLine("-- GeoCountries seed");
countrySql.AppendLine("DELETE FROM GeoCountries;");
countrySql.AppendLine("SET IDENTITY_INSERT GeoCountries OFF;");

var countryText = File.ReadAllText(Path.Combine(cacheDir, "countryInfo.txt"));
var countryMap  = new Dictionary<string, int>(); // code -> geonameId

foreach (var line in countryText.Split('\n'))
{
    if (line.StartsWith('#') || string.IsNullOrWhiteSpace(line)) continue;
    var cols = line.Split('\t');
    if (cols.Length < 17 || !int.TryParse(cols[16].Trim(), out var id)) continue;
    var code   = cols[0].Trim().Replace("'", "''");
    var nameEn = cols[4].Trim().Replace("'", "''");
    var nameAr = (arNames.TryGetValue(id, out var ar) ? ar : nameEn).Replace("'", "''");
    countryMap[cols[0].Trim()] = id;
    countrySql.AppendLine($"INSERT INTO GeoCountries (GeonameId, CountryCode, NameEn, NameAr) VALUES ({id}, '{code}', N'{nameEn}', N'{nameAr}');");
}

File.WriteAllText("geo_countries.sql", countrySql.ToString(), Encoding.UTF8);
Console.WriteLine("Written: geo_countries.sql");

// ── States ────────────────────────────────────────────────────────────────────
var stateSql  = new StringBuilder();
stateSql.AppendLine("-- GeoStates seed");
stateSql.AppendLine("DELETE FROM GeoStates;");

var stateText = File.ReadAllText(Path.Combine(cacheDir, "admin1CodesASCII.txt"));
foreach (var line in stateText.Split('\n'))
{
    if (string.IsNullOrWhiteSpace(line)) continue;
    var cols = line.Split('\t');
    if (cols.Length < 4 || !int.TryParse(cols[3].Trim(), out var id)) continue;
    var countryCode = cols[0].Trim().Split('.')[0];
    if (!countryMap.TryGetValue(countryCode, out var countryId)) continue;
    var nameEn = cols[1].Trim().Replace("'", "''");
    var nameAr = (arNames.TryGetValue(id, out var ar) ? ar : nameEn).Replace("'", "''");
    stateSql.AppendLine($"INSERT INTO GeoStates (GeonameId, CountryGeonameId, NameEn, NameAr) VALUES ({id}, {countryId}, N'{nameEn}', N'{nameAr}');");
}

File.WriteAllText("geo_states.sql", stateSql.ToString(), Encoding.UTF8);
Console.WriteLine("Written: geo_states.sql");
Console.WriteLine("Done! Run these SQL files on the server DB manager.");
