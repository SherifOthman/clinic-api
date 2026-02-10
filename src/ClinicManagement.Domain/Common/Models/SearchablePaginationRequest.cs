namespace ClinicManagement.Domain.Common.Models;

/// <summary>
/// Enhanced pagination request with search and filtering capabilities
/// </summary>
public class SearchablePaginationRequest : PaginationRequest
{
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public string SortDirection { get; set; } = "asc";
    public Dictionary<string, object> Filters { get; set; } = new();
    public bool IsAscending => SortDirection.ToLower() != "desc";
}
