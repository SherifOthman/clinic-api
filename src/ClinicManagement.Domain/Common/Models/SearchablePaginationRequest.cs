namespace ClinicManagement.Domain.Common.Models;

/// <summary>
/// Enhanced pagination request with search and filtering capabilities
/// </summary>
public class SearchablePaginationRequest : PaginationRequest
{
    /// <summary>
    /// Search term to filter results
    /// </summary>
    public string? SearchTerm { get; set; }
    
    /// <summary>
    /// Sort field name
    /// </summary>
    public string? SortBy { get; set; }
    
    /// <summary>
    /// Sort direction (asc/desc)
    /// </summary>
    public string SortDirection { get; set; } = "asc";
    
    /// <summary>
    /// Additional filters as key-value pairs
    /// </summary>
    public Dictionary<string, object> Filters { get; set; } = new();
    
    /// <summary>
    /// Check if sorting is ascending
    /// </summary>
    public bool IsAscending => SortDirection.ToLower() != "desc";
}