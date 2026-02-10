using Microsoft.EntityFrameworkCore;
using ClinicManagement.Domain.Common.Constants;

namespace ClinicManagement.Infrastructure.Extensions;

public static class QueryFilterExtensions
{
    public static IQueryable<T> IgnoreAllFilters<T>(this IQueryable<T> query) where T : class
    {
        return query.IgnoreQueryFilters();
    }

    public static IQueryable<T> IgnoreTenantFilter<T>(this IQueryable<T> query) where T : class
    {
        return query.IgnoreQueryFilters(QueryFilterConstants.TenantFilterOnly);
    }

    public static IQueryable<T> IncludeSoftDeleted<T>(this IQueryable<T> query) where T : class
    {
        return query.IgnoreQueryFilters(QueryFilterConstants.SoftDeleteFilterOnly);
    }
}
