namespace ClinicManagement.API.Models;

/// <summary>
/// Base class for paginated [FromQuery] requests.
/// ASP.NET Core model binding maps query string params to these properties automatically.
///
/// Guards:
///   PageNumber — minimum 1 (negative/zero silently corrected)
///   PageSize   — clamped to [1, 100] (prevents ?pageSize=999999 DoS)
/// </summary>
public class PaginationRequest
{
    private int _pageNumber = 1;
    private int _pageSize   = 10;

    public int PageNumber
    {
        get => _pageNumber;
        init => _pageNumber = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value < 1 ? 10 : value > 100 ? 100 : value;
    }
}

public class SortedPaginationRequest : PaginationRequest
{
    public string? SortBy        { get; init; }
    public string? SortDirection { get; init; }
}
