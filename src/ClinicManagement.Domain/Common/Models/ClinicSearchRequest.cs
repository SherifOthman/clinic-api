namespace ClinicManagement.Domain.Common.Models;

public class ClinicSearchRequest : PaginationRequest
{
    public int? SubscriptionPlanId { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public int? MinUsers { get; set; }
    public int? MaxUsers { get; set; }

    public ClinicSearchRequest()
    {
    }

    public ClinicSearchRequest(int pageNumber, int pageSize, string? searchTerm = null, string? sortBy = null, bool sortDescending = false)
        : base(pageNumber, pageSize)
    {
        SearchTerm = searchTerm;
        SortBy = sortBy;
        SortDescending = sortDescending;
    }

    public bool HasSubscriptionPlanFilter => SubscriptionPlanId.HasValue;
    public bool HasActiveFilter => IsActive.HasValue;
    public bool HasCreatedDateFilter => CreatedFrom.HasValue || CreatedTo.HasValue;
    public bool HasUserCountFilter => MinUsers.HasValue || MaxUsers.HasValue;
    public bool HasSearchTerm => !string.IsNullOrWhiteSpace(SearchTerm);
}