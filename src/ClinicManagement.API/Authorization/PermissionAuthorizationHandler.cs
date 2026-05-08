using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Entities;
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
/// IServiceScopeFactory is injected instead of IPermissionRepository directly
/// because this handler is registered as Singleton while IPermissionRepository
/// is Scoped (EF Core). IServiceScopeFactory is more explicit than IServiceProvider
/// about the intent: we need a new scope per authorization call.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceScopeFactory _scopeFactory;

    public PermissionAuthorizationHandler(IServiceScopeFactory scopeFactory)
        => _scopeFactory = scopeFactory;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        // ClinicOwner has implicit access to all permissions — they own the clinic
        if (context.User.IsInRole(UserRoles.ClinicOwner))
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

        // Create a short-lived scope to resolve the scoped IPermissionRepository.
        // IServiceScopeFactory is the correct abstraction here — it makes the
        // intent explicit and avoids the Service Locator anti-pattern of IServiceProvider.
        await using var scope = _scopeFactory.CreateAsyncScope();
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
