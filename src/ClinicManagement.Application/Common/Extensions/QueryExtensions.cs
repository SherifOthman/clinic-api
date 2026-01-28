using System.Linq.Expressions;

namespace ClinicManagement.Application.Common.Extensions;

public static class QueryExtensions
{
    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> query, 
        bool condition, 
        Expression<Func<T, bool>> predicate)
    {
        return condition ? query.Where(predicate) : query;
    }

    public static IQueryable<T> Paginate<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}