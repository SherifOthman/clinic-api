using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.API.Common.Extensions;

/// <summary>
/// Extension methods for working with query filters (soft delete, multi-tenancy)
/// </summary>
public static class QueryFilterExtensions
{
    /// <summary>
    /// Includes soft-deleted entities in the query
    /// Use for admin operations or restore functionality
    /// </summary>
    public static IQueryable<T> IncludeDeleted<T>(this IQueryable<T> query) where T : class
    {
        return query.IgnoreQueryFilters();
    }
    
    /// <summary>
    /// Gets only soft-deleted entities
    /// Use for "trash" or "recycle bin" functionality
    /// </summary>
    public static IQueryable<T> OnlyDeleted<T>(this IQueryable<T> query) where T : AuditableEntity
    {
        return query.IgnoreQueryFilters().Where(e => e.IsDeleted);
    }
    
    /// <summary>
    /// Includes entities from all tenants (bypasses multi-tenancy filter)
    /// Use for SuperAdmin operations only
    /// </summary>
    public static IQueryable<T> IncludeAllTenants<T>(this IQueryable<T> query) where T : class
    {
        return query.IgnoreQueryFilters();
    }
}
