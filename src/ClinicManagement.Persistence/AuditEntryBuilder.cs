using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ClinicManagement.Persistence;

/// <summary>
/// Builds AuditLog entries from EF Core change tracker state.
/// Extracted from ApplicationDbContext to keep it focused on data access.
/// </summary>
internal sealed class AuditEntryBuilder
{
    public List<AuditLog> Build(
        IEnumerable<EntityEntry<AuditableEntity>> entries,
        Guid? userId,
        string? fullName,
        string? username,
        string? userEmail,
        string? userRole,
        string? ipAddress,
        string? userAgent,
        DateTimeOffset now)
    {
        var logs = new List<AuditLog>();

        foreach (var entry in entries)
        {
            if (!IsTrackable(entry)) continue;

            var action  = GetAction(entry);
            var changes = action == AuditAction.Update
                ? GetUpdatedFields(entry)
                : GetAllFields(entry);

            logs.Add(new AuditLog
            {
                Timestamp  = now,
                UserId     = userId,
                FullName   = fullName,
                Username   = username,
                UserEmail  = userEmail,
                UserRole   = userRole,
                IpAddress  = ipAddress,
                UserAgent  = userAgent,
                EntityType = entry.Entity.GetType().Name,
                EntityId   = entry.Entity.Id.ToString(),
                Action     = action,
                Changes    = Serialize(changes),
            });
        }

        return logs;
    }

    private static bool IsTrackable(EntityEntry<AuditableEntity> entry)
        => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted
           && entry.Entity is not INoAuditLog;

    private static AuditAction GetAction(EntityEntry<AuditableEntity> entry)
        => entry.State switch
        {
            EntityState.Added   => AuditAction.Create,
            EntityState.Deleted => AuditAction.Delete,
            _                   => AuditAction.Update,
        };

    private static Dictionary<string, object?> GetUpdatedFields(EntityEntry entry)
    {
        var result = new Dictionary<string, object?>();
        foreach (var prop in entry.Properties)
        {
            if (!prop.IsModified) continue;
            if (Equals(prop.OriginalValue, prop.CurrentValue)) continue;
            result[prop.Metadata.Name] = new { Old = prop.OriginalValue, New = prop.CurrentValue };
        }
        return result;
    }

    private static Dictionary<string, object?> GetAllFields(EntityEntry entry)
    {
        var result = new Dictionary<string, object?>();
        foreach (var prop in entry.Properties)
            result[prop.Metadata.Name] = prop.CurrentValue;
        return result;
    }

    private static string? Serialize(Dictionary<string, object?> data)
        => data.Count == 0 ? null : System.Text.Json.JsonSerializer.Serialize(data);
}
