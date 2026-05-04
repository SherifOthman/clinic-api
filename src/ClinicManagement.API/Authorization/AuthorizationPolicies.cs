namespace ClinicManagement.API.Authorization;

/// <summary>
/// Named authorization policy constants.
/// Use these instead of inline strings to get compile-time safety.
/// Matches the policy names registered in DependencyInjection.AddAuthorization.
/// </summary>
public static class AuthorizationPolicies
{
    public const string ClinicOwner = "RequireClinicOwner";
    public const string SuperAdmin  = "SuperAdmin";
    public const string Cors        = "AllowAll";
}
