using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace ClinicManagement.API.Authorization;

/// <summary>
/// Handles permission-based authorization by resolving the member's permissions
/// from IMemoryCache (via IPermissionRepository) using the MemberId JWT claim.
///
/// Permissions are NOT stored in the JWT — only MemberId is.
/// This keeps tokens small and avoids stale permission data.
///
/// Cache hit  = zero DB queries per request (same performance as JWT claims).
/// Cache miss = one DB query, then cached for 10 minutes.
///
/// IServiceProvider is used instead of injecting IPermissionRepository directly
/// because this handler is registered as Singleton while IPermissionRepository
/// is Scoped (EF Core). A new scope is created per authorization call.
/// </summary>
public class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceProvider _services;

    public PermissionAuthorizationHandler(IServiceProvider services)
        => _services = services;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // ClinicOwner has implicit access to all permissions — they own the clinic
        if (context.User.IsInRole("ClinicOwner"))
        {
            context.Succeed(requirement);
            return;
        }

        var memberIdClaim = context.User.FindFirst("MemberId")?.Value;

        // No MemberId — user has no clinic membership (SuperAdmin, unregistered user)
        if (!Guid.TryParse(memberIdClaim, out var memberId))
            return;

        if (!Enum.TryParse<Permission>(requirement.Permission, out var permission))
            return;

        // Create a short-lived scope to resolve the scoped IPermissionRepository
        await using var scope = _services.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPermissionRepository>();

        var memberPermissions = await repo.GetByMemberIdAsync(memberId);

        if (memberPermissions.Contains(permission))
            context.Succeed(requirement);
    }
}

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public PermissionRequirement(string permission) => Permission = permission;
}
