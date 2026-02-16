using System.Linq.Expressions;

namespace ClinicManagement.API.Common.Extensions;

/// <summary>
/// Extension methods for building queries with common patterns
/// </summary>
public static class QueryExtensions
{
    /// <summary>
    /// Conditionally applies a where clause
    /// </summary>
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query,
        bool condition,
        Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }

    /// <summary>
    /// Applies pagination (skip and take)
    /// </summary>
    public static IQueryable<T> Paginate<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }

    /// <summary>
    /// Searches across multiple string properties
    /// </summary>
    public static IQueryable<T> SearchBy<T>(
        this IQueryable<T> query,
        string? searchTerm,
        params Expression<Func<T, string>>[] properties)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || properties.Length == 0)
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combinedExpression = null;

        foreach (var property in properties)
        {
            var propertyAccess = Expression.Invoke(property, parameter);
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
            var containsExpression = Expression.Call(
                propertyAccess,
                containsMethod,
                Expression.Constant(searchTerm));

            combinedExpression = combinedExpression == null
                ? containsExpression
                : Expression.OrElse(combinedExpression, containsExpression);
        }

        var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression!, parameter);
        return query.Where(lambda);
    }
}
