namespace ClinicManagement.Application.Extensions;

public static class PaginationExtensions
{
    public static IQueryable<TSource> Paginate<TSource>(this IQueryable<TSource> source, int pageNumber, int pageSize)
    {
        return source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}