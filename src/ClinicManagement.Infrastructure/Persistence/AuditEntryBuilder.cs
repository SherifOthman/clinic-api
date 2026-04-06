using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ClinicManagement.Infrastructure.Persistence;

/// <summary>
/// Builds AuditLog entries from EF Core change tracker state.
/// Extracted from ApplicationDbContext to keep it focused on data access.
/// </summary>
internal sealed class AuditEntryBuilder
{
    private readonly IMemoryCache? _cache;

    public AuditEntryBuilder(IMemoryCache? cache)
    {
        _cache = cache;
    }

    public List<AuditLog> Build(
        IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<AuditableEntity>> entries,
        Guid? userId, string? fullName, string? username,
        string? userEmail, string? userAgent, string? ipAddress,
        string? userRole, DateTime now)
    {
        var logs = new List<AuditLog>();

        foreach (var entry in entries)
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            if (entry.Entity is RefreshToken) continue;
            if (entry.Entity is INoAuditLog) continue;

            var action = ResolveAction(entry);

            var skipFields = new HashSet<string>
            {
                "Id", "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy", "IsDeleted",
                "ClinicId", "UserId", "PatientId", "StaffId", "DoctorProfileId",
                "AppointmentId", "InvoiceId", "MedicalVisitId"
            };

            string? changesJson = action switch
            {
                AuditAction.Update  => BuildUpdateChanges(entry.Properties, skipFields),
                AuditAction.Create  => BuildSnapshot(entry.Properties, skipFields, useOriginal: false),
                _                   => BuildSnapshot(entry.Properties, skipFields, useOriginal: entry.State == EntityState.Deleted),
            };

            Guid? clinicId = entry.Entity is ITenantEntity tenant ? tenant.ClinicId : null;

            logs.Add(new AuditLog
            {
                Timestamp  = now,
                ClinicId   = clinicId,
                UserId     = userId,
                FullName   = fullName,
                Username   = username,
                UserEmail  = userEmail,
                UserRole   = userRole,
                UserAgent  = userAgent,
                EntityType = entry.Entity.GetType().Name,
                EntityId   = entry.Entity.Id.ToString(),
                Action     = action,
                IpAddress  = ipAddress,
                Changes    = changesJson,
            });
        }

        return logs;
    }

    private static AuditAction ResolveAction(
        Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<AuditableEntity> entry)
    {
        if (entry.State == EntityState.Added) return AuditAction.Create;
        if (entry.State == EntityState.Deleted) return AuditAction.Delete;

        var isDeletedProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "IsDeleted");
        if (isDeletedProp is { IsModified: true })
        {
            var wasDeleted = isDeletedProp.OriginalValue is true;
            var isNowDeleted = isDeletedProp.CurrentValue is true;
            return (wasDeleted, isNowDeleted) switch
            {
                (false, true)  => AuditAction.Delete,
                (true,  false) => AuditAction.Restore,
                _              => AuditAction.Update,
            };
        }

        return AuditAction.Update;
    }

    private string? BuildUpdateChanges(
        IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry> properties,
        HashSet<string> skipFields)
    {
        var changes = new Dictionary<string, object>();
        foreach (var prop in properties)
        {
            if (!prop.IsModified) continue;
            if (skipFields.Contains(prop.Metadata.Name)) continue;
            var old = HumanizeValue(prop.Metadata.Name, prop.OriginalValue);
            var @new = HumanizeValue(prop.Metadata.Name, prop.CurrentValue);
            if (old is null && @new is null) continue;
            changes[GetFieldLabel(prop.Metadata.Name)] = new { Old = old, New = @new };
        }
        return changes.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(changes) : null;
    }

    private string? BuildSnapshot(
        IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry> properties,
        HashSet<string> skipFields,
        bool useOriginal)
    {
        var raw = new Dictionary<string, object?>();
        foreach (var prop in properties)
        {
            if (skipFields.Contains(prop.Metadata.Name)) continue;
            var val = useOriginal ? prop.OriginalValue : prop.CurrentValue;
            if (val is null) continue;
            var humanized = HumanizeValue(prop.Metadata.Name, val);
            if (humanized is null) continue;
            raw[prop.Metadata.Name] = humanized;
        }

        var result = new Dictionary<string, object?>();
        var locationFields = new[] { "CountryGeoNameId", "StateGeoNameId", "CityGeoNameId" };
        var fieldOrder = new[] { "FullName", "IsMale", "DateOfBirth", "BloodType", "PatientCode",
                                  "Name", "Email", "PhoneNumber", "Role", "IsActive", "IsRevoked", "ExpiryTime", "EVENT" };

        foreach (var field in fieldOrder)
            if (raw.TryGetValue(field, out var v))
                result[GetFieldLabel(field)] = v;

        var locationParts = new List<string>();
        if (raw.TryGetValue("CountryGeoNameId", out var country) && country is string cn) locationParts.Add(cn);
        if (raw.TryGetValue("StateGeoNameId",   out var state)   && state   is string sn) locationParts.Add(sn);
        if (raw.TryGetValue("CityGeoNameId",     out var city)    && city    is string ct) locationParts.Add(ct);
        if (locationParts.Count > 0)
            result["Location"] = string.Join(" › ", locationParts);

        foreach (var (key, val) in raw)
        {
            if (fieldOrder.Contains(key)) continue;
            if (locationFields.Contains(key)) continue;
            result[GetFieldLabel(key)] = val;
        }

        return result.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(result) : null;
    }

    private static string GetFieldLabel(string fieldName) => fieldName switch
    {
        "FullName"           => "Full Name",
        "IsMale"             => "Gender",
        "DateOfBirth"        => "Date of Birth",
        "BloodType"          => "Blood Type",
        "PatientCode"        => "Patient Code",
        "CountryGeoNameId"   => "Country",
        "StateGeoNameId"     => "State",
        "CityGeoNameId"      => "City",
        "IsActive"           => "Status",
        "IsRevoked"          => "Token Status",
        "ExpiryTime"         => "Expiry",
        "PhoneNumber"        => "Phone",
        "Role"               => "Role",
        "Name"               => "Name",
        "Email"              => "Email",
        "EVENT"              => "Event",
        _                    => System.Text.RegularExpressions.Regex.Replace(fieldName, "([A-Z])", " $1").Trim()
    };

    private object? HumanizeValue(string fieldName, object? value)
    {
        if (value is null) return null;

        return fieldName switch
        {
            "IsMale"    => value is bool b ? (b ? "Male" : "Female") : value,
            "IsActive"  => value is bool a ? (a ? "Active" : "Inactive") : value,
            "IsDeleted" => value is bool d ? (d ? "Deleted" : "Active") : value,
            "IsRevoked" => value is bool r ? (r ? "Revoked" : "Active") : value,

            "BloodType" => value switch
            {
                BloodType bt => bt.ToDisplayString(),
                int i => i switch
                {
                    1 => "A+", 2 => "A-", 3 => "B+", 4 => "B-",
                    5 => "AB+", 6 => "AB-", 7 => "O+", 8 => "O-",
                    _ => value
                },
                _ => value
            },

            "DateOfBirth" => value is DateTime dt ? dt.ToString("yyyy-MM-dd") : value,

            "CountryGeoNameId" => ResolveLocationName("countries", value),
            "StateGeoNameId"   => ResolveLocationName(null, value),
            "CityGeoNameId"    => ResolveLocationName(null, value),

            "ClinicId" or "UserId" or "PatientId" or "StaffId" or
            "CreatedBy" or "UpdatedBy" or "Id" => null,

            _ => value
        };
    }

    private object? ResolveLocationName(string? countryCacheKey, object? value)
    {
        if (value is null || _cache is null) return value;

        int geoId;
        if (value is int i) geoId = i;
        else if (value is long l) geoId = (int)l;
        else return value;

        if (countryCacheKey != null &&
            _cache.TryGetValue(countryCacheKey, out List<GeoNamesCountry>? countries) &&
            countries != null)
        {
            var match = countries.FirstOrDefault(c => c.GeonameId == geoId);
            if (match != null) return match.Name.En;
        }

        if (_cache.TryGetValue($"_geoname_{geoId}", out string? cachedName) && cachedName != null)
            return cachedName;

        return $"#{geoId}";
    }
}
