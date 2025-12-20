using Microsoft.AspNetCore.Authorization;

namespace ClinicManagement.Application.Common.Authorization.Requirements;

/// <summary>
/// Requirement that user must be the owner of their clinic.
/// Only clinic owners can perform certain sensitive operations like:
/// - Inviting staff
/// - Managing subscriptions
/// - Deleting clinic data
/// </summary>
public class ClinicOwnerRequirement : IAuthorizationRequirement
{
}
