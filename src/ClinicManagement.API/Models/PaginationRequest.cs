using ClinicManagement.Application.Common.Models;

namespace ClinicManagement.API.Models;

/// <summary>
/// Base class for paginated [FromQuery] requests.
/// Guards: PageNumber >= 1, PageSize clamped to [1, 100].
/// </summary>
public class PaginationRequest
{
    public int PageNumber
    {
        get;
        init => field = value < 1 ? 1 : value;
    } = 1;

    public int PageSize
    {
        get;
        init => field = value < 1 ? 10 : value > 100 ? 100 : value;
    } = 10;
}

public class SortedPaginationRequest : PaginationRequest
{
    public string?      SortBy        { get; init; }
    /// <summary>
    /// Accepts "asc" or "desc" (case-insensitive). Defaults to Asc for any other value.
    /// Parsed to the strongly-typed SortDirection enum so callers never deal with raw strings.
    /// </summary>
    public SortDirection SortDirection { get; init; } = SortDirection.Asc;
}
