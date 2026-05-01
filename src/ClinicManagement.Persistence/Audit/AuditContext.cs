using ClinicManagement.Application.Abstractions.Services;

namespace ClinicManagement.Persistence.Audit;

/// <summary>
/// Snapshot of the current user's identity at the moment SaveChanges is called.
/// Passed to AuditChangeTracker so it doesn't need a direct reference to ICurrentUserService.
/// </summary>
internal sealed record AuditContext(
    Guid?   UserId,
    Guid?   ClinicId,
    string? FullName,
    string? Username,
    string? Email,
    string? Role,
    string? IpAddress,
    string? UserAgent)
{
    internal static AuditContext From(ICurrentUserService? user) => new(
        UserId:    user?.UserId,
        ClinicId:  user?.ClinicId,
        FullName:  user?.FullName,
        Username:  user?.Username,
        Email:     user?.Email,
        Role:      user?.Roles.FirstOrDefault(),
        IpAddress: user?.IpAddress,
        UserAgent: user?.UserAgent);
}
