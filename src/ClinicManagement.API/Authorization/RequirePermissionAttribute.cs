using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace ClinicManagement.API.Authorization;

/// <summary>
/// Requires the authenticated user to have the specified permission claim in their JWT.
/// Usage: [RequirePermission(Permission.CreatePatient)]
/// </summary>
public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(Permission permission)
        : base($"Permission:{permission}")
    {
    }
}
