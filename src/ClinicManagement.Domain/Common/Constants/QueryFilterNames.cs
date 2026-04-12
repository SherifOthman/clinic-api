namespace ClinicManagement.Domain.Common.Constants;

/// <summary>
/// Named query filter keys for EF Core named filters.
/// Use with IgnoreQueryFilters(["FilterName"]) to selectively bypass.
/// </summary>
public static class QueryFilterNames
{
    /// <summary>Filters tenant entities to the current user's ClinicId.</summary>
    public const string Tenant = "TenantFilter";

    /// <summary>Filters soft-deleted entities.</summary>
    public const string SoftDelete = "SoftDeleteFilter";
}
