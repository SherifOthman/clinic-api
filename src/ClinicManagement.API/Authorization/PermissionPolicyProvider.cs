using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ClinicManagement.API.Authorization;

/// <summary>
/// Generates authorization policies on-demand for any Permission enum value.
///
/// Replaces the static foreach loop in DependencyInjection.cs.
/// Any new permission added to the enum is automatically available as a policy
/// without touching DI configuration.
///
/// Policy name format: "Permission:{EnumValue}" — matches RequirePermissionAttribute.
/// All other policy names (RequireClinicOwner, SuperAdmin) fall through to the
/// default provider.
/// </summary>
public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private const string PermissionPolicyPrefix = "Permission:";

    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        => _fallback = new DefaultAuthorizationPolicyProvider(options);

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.StartsWith(PermissionPolicyPrefix, StringComparison.Ordinal))
            return _fallback.GetPolicyAsync(policyName);

        var permissionName = policyName[PermissionPolicyPrefix.Length..];

        // Validate it's a known permission — reject unknown strings
        if (!Enum.TryParse<Permission>(permissionName, out _))
            return _fallback.GetPolicyAsync(policyName);

        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(permissionName))
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => _fallback.GetFallbackPolicyAsync();
}
