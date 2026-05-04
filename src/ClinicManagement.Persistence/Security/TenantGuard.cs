using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence.Security;

/// <summary>
/// Safe wrapper around IgnoreQueryFilters for cross-tenant (admin) queries.
///
/// RULES:
///   ✅ Call AsAdminQuery() only from Admin repository methods (GetAdmin*, CountIgnore*, etc.)
///   ❌ Never call DbSet.IgnoreQueryFilters() directly in clinic-facing code
///   ❌ Never call AsAdminQuery() from Application/Features/Patients/ handlers
///
/// If called without SuperAdmin role → throws UnauthorizedAccessException at runtime.
/// If called from wrong namespace → caught by the Roslyn analyzer at compile time.
///
/// This makes cross-tenant access impossible to do accidentally.
/// </summary>
public static class TenantGuard
{
    /// <summary>
    /// Returns a cross-tenant queryable. Throws if the current user is not SuperAdmin.
    /// Use only in Admin repository methods.
    /// </summary>
    public static IQueryable<TEntity> AsAdminQuery<TEntity>(
        DbSet<TEntity> dbSet,
        ICurrentUserService currentUser)
        where TEntity : class
    {
        // Runtime guard — belt-and-suspenders on top of the [Authorize("SuperAdmin")] attribute
        if (!currentUser.Roles.Contains(UserRoles.SuperAdmin))
            throw new UnauthorizedAccessException(
                $"Cross-tenant query attempted by non-admin user {currentUser.UserId}. " +
                "This is a developer error — AsAdminQuery() must only be called from admin repository methods.");

        return dbSet.IgnoreQueryFilters([QueryFilterNames.Tenant]);
    }

    /// <summary>
    /// Returns a cross-tenant queryable for background jobs and system services
    /// that have no HTTP context (Hangfire jobs, seeders, etc.).
    /// Does NOT check roles — caller is responsible for ensuring this is a system context.
    /// </summary>
    public static IQueryable<TEntity> AsSystemQuery<TEntity>(DbSet<TEntity> dbSet)
        where TEntity : class
        => dbSet.IgnoreQueryFilters([QueryFilterNames.Tenant]);

    /// <summary>
    /// Returns a queryable that ignores ALL filters (tenant + soft-delete).
    /// Use only for restore operations or existence checks that must see deleted records.
    /// </summary>
    public static IQueryable<TEntity> AsUnfilteredQuery<TEntity>(DbSet<TEntity> dbSet)
        where TEntity : class
        => dbSet.IgnoreQueryFilters();
}
