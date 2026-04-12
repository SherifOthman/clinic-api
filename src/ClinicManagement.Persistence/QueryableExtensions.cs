using ClinicManagement.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Persistence;

/// <summary>
/// Extension methods on IQueryable that encapsulate patterns repeated across all repositories.
///
/// ToPagedAsync  — runs CountAsync, then Skip/Take, then ToListAsync, and wraps in PaginatedResult.
/// IsDescending  — parses a sort direction string ("asc" / "desc") into a bool.
///
/// Usage in a repo:
///   var query = DbSet.AsNoTracking().Where(...).OrderBy(...);
///   return await query.ToPagedAsync(pageNumber, pageSize, ct);
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Counts the full filtered set, then fetches one page, and returns a PaginatedResult.
    /// Always call this AFTER applying all filters and sorting.
    /// </summary>
    public static async Task<PaginatedResult<T>> ToPagedAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var totalCount = await query.CountAsync(ct);
        var skip       = (pageNumber - 1) * pageSize;
        var items      = await query.Skip(skip).Take(pageSize).ToListAsync(ct);

        return PaginatedResult<T>.Create(items, totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// Returns true when sortDirection is "desc" (case-insensitive).
    /// Defaults to ascending for any other value including null.
    ///
    /// Usage:
    ///   var desc = sortDirection.IsDescending();
    ///   query = desc ? query.OrderByDescending(...) : query.OrderBy(...);
    /// </summary>
    public static bool IsDescending(this string? sortDirection)
        => string.Equals(sortDirection?.Trim(), "desc", StringComparison.OrdinalIgnoreCase);
}
