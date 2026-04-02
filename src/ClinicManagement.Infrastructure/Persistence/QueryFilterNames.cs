namespace ClinicManagement.Infrastructure.Persistence;

/// <summary>
/// Named query filter identifiers for EF Core 10 named filters.
/// Use these constants with IgnoreQueryFilters(["FilterName"]) to selectively bypass filters.
/// </summary>
public static class QueryFilterNames
{
    /// <summary>
    /// Filters all ITenantEntity types to the current user's ClinicId.
    /// Bypass with: query.IgnoreQueryFilters([QueryFilterNames.Clinic])
    /// </summary>
    public const string Clinic = "ClinicFilter";
}
