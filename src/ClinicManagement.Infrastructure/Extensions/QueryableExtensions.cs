using Microsoft.EntityFrameworkCore;

namespace ClinicManagement.Infrastructure.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<T> Paginate<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }

    public static async Task<(List<T> Items, int TotalCount)> ToPaginatedListAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Paginate(pageNumber, pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
