using Microsoft.AspNetCore.Authorization;

namespace ClinicManagement.Application.Common.Authorization.Requirements;

/// <summary>
/// Requirement that user must belong to the same clinic as the resource being accessed.
/// </summary>
public class SameClinicRequirement : IAuthorizationRequirement
{
    // This requirement has no additional properties
    // The handler will check if user's ClinicId matches the resource's ClinicId
}
