namespace ClinicManagement.Domain.Common.Models;

public class UserSearchRequest : PaginationRequest
{
    public bool? EmailConfirmed { get; set; }
    public string? Role { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public DateTime? LastLoginFrom { get; set; }
    public DateTime? LastLoginTo { get; set; }

    public UserSearchRequest()
    {
    }

    public UserSearchRequest(int pageNumber, int pageSize, string? searchTerm = null, string? sortBy = null, bool sortDescending = false)
        : base(pageNumber, pageSize)
    {
        SearchTerm = searchTerm;
        SortBy = sortBy;
        SortDescending = sortDescending;
    }

    public bool HasEmailConfirmedFilter => EmailConfirmed.HasValue;
    public bool HasRoleFilter => !string.IsNullOrWhiteSpace(Role);
    public bool HasCreatedDateFilter => CreatedFrom.HasValue || CreatedTo.HasValue;
    public bool HasLastLoginFilter => LastLoginFrom.HasValue || LastLoginTo.HasValue;
    public bool HasSearchTerm => !string.IsNullOrWhiteSpace(SearchTerm);
}