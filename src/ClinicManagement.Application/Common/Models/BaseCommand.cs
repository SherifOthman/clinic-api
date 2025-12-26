using ClinicManagement.Application.Common.Interfaces;

namespace ClinicManagement.Application.Common.Models;

/// <summary>
/// Base class for commands that need current user context
/// </summary>
public abstract class BaseCommand
{
    /// <summary>
    /// Gets the current user ID from the context
    /// </summary>
    protected int GetCurrentUserId(ICurrentUserService currentUserService)
    {
        currentUserService.EnsureAuthenticated();
        return currentUserService.GetRequiredUserId();
    }

    /// <summary>
    /// Gets the current clinic ID from the context
    /// </summary>
    protected int GetCurrentClinicId(ICurrentUserService currentUserService)
    {
        currentUserService.EnsureClinicAccess();
        return currentUserService.GetRequiredClinicId();
    }

    /// <summary>
    /// Gets both current user and clinic IDs
    /// </summary>
    protected (int UserId, int ClinicId) GetCurrentUserAndClinicIds(ICurrentUserService currentUserService)
    {
        currentUserService.EnsureClinicAccess();
        return (currentUserService.GetRequiredUserId(), currentUserService.GetRequiredClinicId());
    }
}