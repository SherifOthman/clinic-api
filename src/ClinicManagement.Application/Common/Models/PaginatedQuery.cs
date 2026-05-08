namespace ClinicManagement.Application.Common.Models;

public abstract record PaginatedQuery
{
    /// <summary>Always >= 1. Values below 1 are clamped to 1.</summary>
    public int PageNumber { get; init; }

    /// <summary>Clamped to 1–100. Prevents runaway queries from callers sending PageSize=10000.</summary>
    public int PageSize { get; init; }

    protected PaginatedQuery(int pageNumber = 1, int pageSize = 10)
    {
        PageNumber = Math.Max(1, pageNumber);
        PageSize   = Math.Clamp(pageSize, 1, 100);
    }
}
