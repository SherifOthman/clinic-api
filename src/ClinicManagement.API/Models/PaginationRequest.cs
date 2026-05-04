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
    public string? SortBy        { get; init; }
    public string? SortDirection { get; init; }
}
