namespace ClinicManagement.Domain.Common.Constants;

public static class QueryFilterConstants
{
    public const string SoftDeleteFilter = "SoftDeleteFilter";
    public const string TenantFilter = "TenantFilter";

    public static readonly string[] AllFilters = { SoftDeleteFilter, TenantFilter };
    public static readonly string[] TenantFilterOnly = { TenantFilter };
    public static readonly string[] SoftDeleteFilterOnly = { SoftDeleteFilter };
}
