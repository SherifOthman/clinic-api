namespace ClinicManagement.API.Models;

/// <summary>
/// Base class for paginated [FromQuery] requests.
/// ASP.NET Core model binding maps query string params to these properties automatically.
/// </summary>
public class PaginationRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public class SortedPaginationRequest : PaginationRequest
{
    public string? SortBy { get; init; }
    public string? SortDirection { get; init; }
}
