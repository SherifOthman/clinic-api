using ClinicManagement.Domain.Common.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ClinicManagement.Infrastructure.Extensions;

/// <summary>
/// Extension methods for IQueryable to support pagination, search, and filtering
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Converts an IQueryable to a paged result
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// Converts an IQueryable to a paged result with pagination request
    /// </summary>
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PaginationRequest paginationRequest,
        CancellationToken cancellationToken = default)
    {
        return await query.ToPagedResultAsync(
            paginationRequest.PageNumber,
            paginationRequest.PageSize,
            cancellationToken);
    }

    /// <summary>
    /// Converts an IQueryable to a paged result with DTO mapping using Mapster
    /// </summary>
    public static async Task<PagedResult<TDto>> ToPagedResultAsync<TEntity, TDto>(
        this IQueryable<TEntity> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        var dtos = items.Adapt<List<TDto>>();
        
        return new PagedResult<TDto>(dtos, totalCount, pageNumber, pageSize);
    }

    /// <summary>
    /// Converts an IQueryable to a paged result with DTO mapping and SearchablePaginationRequest
    /// </summary>
    public static async Task<PagedResult<TDto>> ToPagedResultAsync<TEntity, TDto>(
        this IQueryable<TEntity> query,
        SearchablePaginationRequest request,
        CancellationToken cancellationToken = default)
    {
        var totalCount = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);
        
        var dtos = items.Adapt<List<TDto>>();
        
        return new PagedResult<TDto>(dtos, totalCount, request.PageNumber, request.PageSize);
    }

    /// <summary>
    /// Apply search filter to a query based on multiple properties
    /// </summary>
    public static IQueryable<T> ApplySearch<T>(
        this IQueryable<T> query,
        string? searchTerm,
        params Expression<Func<T, string>>[] searchProperties)
    {
        if (string.IsNullOrWhiteSpace(searchTerm) || searchProperties.Length == 0)
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? combinedExpression = null;

        foreach (var property in searchProperties)
        {
            var propertyAccess = Expression.Invoke(property, parameter);
            var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
            var propertyToLower = Expression.Call(propertyAccess, toLowerMethod!);
            
            var searchTermLower = searchTerm.ToLower();
            var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var containsExpression = Expression.Call(
                propertyToLower,
                containsMethod!,
                Expression.Constant(searchTermLower));

            combinedExpression = combinedExpression == null
                ? containsExpression
                : Expression.OrElse(combinedExpression, containsExpression);
        }

        if (combinedExpression != null)
        {
            var lambda = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
            query = query.Where(lambda);
        }

        return query;
    }

    /// <summary>
    /// Apply dynamic sorting to a query
    /// </summary>
    public static IQueryable<T> ApplySort<T>(
        this IQueryable<T> query,
        string? sortBy,
        bool isAscending = true)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return query;

        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(parameter, sortBy);
        var lambda = Expression.Lambda(property, parameter);

        var methodName = isAscending ? "OrderBy" : "OrderByDescending";
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new[] { typeof(T), property.Type },
            query.Expression,
            Expression.Quote(lambda));

        return query.Provider.CreateQuery<T>(resultExpression);
    }

    /// <summary>
    /// Apply search and sort from SearchablePaginationRequest
    /// </summary>
    public static IQueryable<T> ApplySearchAndSort<T>(
        this IQueryable<T> query,
        SearchablePaginationRequest request,
        params Expression<Func<T, string>>[] searchProperties)
    {
        // Apply search
        query = query.ApplySearch(request.SearchTerm, searchProperties);

        // Apply sort
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            try
            {
                query = query.ApplySort(request.SortBy, request.IsAscending);
            }
            catch
            {
                // If sorting fails (invalid property), just continue without sorting
            }
        }

        return query;
    }
}
