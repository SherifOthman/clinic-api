using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace ClinicManagement.Persistence.Audit;

/// <summary>
/// Responsible for capturing AuditLog entries from EF change tracker entries.
/// Extracted from ApplicationDbContext to keep the context focused on
/// configuration and query filters only.
///
/// Called by ApplicationDbContext.SaveChangesAsync — before the main save
/// (so OriginalValues are still available) and written after (so a failed
/// main save produces no audit entry).
/// </summary>
public static class AuditChangeTracker
{
    /// <summary>
    /// Fields that must never appear in audit diffs.
    /// Hashes, tokens, and EF/Identity internal fields.
    /// </summary>
    private static readonly HashSet<string> ExcludedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
        "NormalizedEmail", "NormalizedUserName",
        "RefreshToken", "TokenHash", "InvitationToken",
        "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy",
    };

    /// <summary>
    /// Scans the change tracker for IAuditableEntity entries and builds
    /// AuditLog objects. Must be called BEFORE base.SaveChangesAsync so
    /// that OriginalValues are still available for Update diffs.
    /// </summary>
    public static List<AuditLog> Capture(
        ChangeTracker changeTracker,
        ICurrentUserService? currentUser,
        DateTimeOffset now)
    {
        var entries = changeTracker
            .Entries<IAuditableEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        if (entries.Count == 0)
            return [];

        var context = AuditContext.From(currentUser);
        var logs    = new List<AuditLog>(entries.Count);

        foreach (var entry in entries)
        {
            var log = BuildLog(entry, context, now);
            if (log is not null)
                logs.Add(log);
        }

        return logs;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static AuditLog? BuildLog(
        EntityEntry<IAuditableEntity> entry,
        AuditContext context,
        DateTimeOffset now)
    {
        var action  = ToAuditAction(entry.State);
        var changes = BuildChangesJson(entry, action);

        // Skip updates where only excluded fields changed (e.g. UpdatedAt stamp)
        if (action == AuditAction.Update && changes is null)
            return null;

        return new AuditLog
        {
            Timestamp  = now,
            ClinicId   = ResolveClinicId(entry, context.ClinicId),
            UserId     = context.UserId,
            FullName   = context.FullName,
            Username   = context.Username,
            UserEmail  = context.Email,
            UserRole   = context.Role,
            IpAddress  = context.IpAddress,
            UserAgent  = context.UserAgent,
            EntityType = entry.Entity.GetType().Name,
            EntityId   = entry.Property("Id").CurrentValue?.ToString() ?? "unknown",
            Action     = action,
            Changes    = changes,
        };
    }

    private static AuditAction ToAuditAction(EntityState state) => state switch
    {
        EntityState.Added   => AuditAction.Create,
        EntityState.Deleted => AuditAction.Delete,
        _                   => AuditAction.Update,
    };

    /// <summary>
    /// Prefer the current user's clinic (from JWT).
    /// Fall back to the entity's own ClinicId for system/seed operations.
    /// </summary>
    private static Guid? ResolveClinicId(EntityEntry<IAuditableEntity> entry, Guid? userClinicId)
        => userClinicId
        ?? (entry.Entity is ITenantEntity tenant ? tenant.ClinicId : null);

    private static string? BuildChangesJson(EntityEntry<IAuditableEntity> entry, AuditAction action)
        => action switch
        {
            AuditAction.Create => SerializeSnapshot(entry, useCurrentValues: true),
            AuditAction.Delete => SerializeSnapshot(entry, useCurrentValues: false),
            _                  => SerializeDiff(entry),
        };

    /// <summary>
    /// Snapshot of all non-excluded, non-null properties.
    /// Used for Create (current values) and Delete (original values).
    /// </summary>
    private static string? SerializeSnapshot(EntityEntry<IAuditableEntity> entry, bool useCurrentValues)
    {
        var snapshot = entry.Properties
            .Where(p => !ExcludedFields.Contains(p.Metadata.Name))
            .Select(p => new
            {
                p.Metadata.Name,
                Value = useCurrentValues ? p.CurrentValue : p.OriginalValue,
            })
            .Where(p => p.Value is not null)
            .ToDictionary(p => p.Name, p => p.Value);

        return snapshot.Count > 0 ? JsonSerializer.Serialize(snapshot) : null;
    }

    /// <summary>
    /// Diff of only the properties that actually changed.
    /// Returns null if nothing meaningful changed.
    /// </summary>
    private static string? SerializeDiff(EntityEntry<IAuditableEntity> entry)
    {
        var diff = entry.Properties
            .Where(p => p.IsModified
                     && !ExcludedFields.Contains(p.Metadata.Name)
                     && !Equals(p.OriginalValue, p.CurrentValue))
            .ToDictionary(
                p => p.Metadata.Name,
                p => new { Old = p.OriginalValue, New = p.CurrentValue });

        return diff.Count > 0 ? JsonSerializer.Serialize(diff) : null;
    }
}
