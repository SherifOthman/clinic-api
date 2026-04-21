using Microsoft.AspNetCore.Authorization;

namespace ClinicManagement.API.Authorization;

/// <summary>
/// Handles permission-based authorization by checking the "permissions" JWT claim.
/// </summary>
public class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var hasClaim = context.User.Claims
            .Any(c => c.Type == "permissions" && c.Value == requirement.Permission);

        if (hasClaim)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public PermissionRequirement(string permission) => Permission = permission;
}
