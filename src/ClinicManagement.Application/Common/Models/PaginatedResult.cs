namespace ClinicManagement.Application.Common.Models;

public record PaginatedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize
)
{
    public int TotalPages       => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage     => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;

    public static PaginatedResult<T> Create(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
        => new(items.ToList(), totalCount, pageNumber, pageSize);
}
