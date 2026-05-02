namespace ClinicManagement.Application.Abstractions.Services;

public interface ICurrentUserService
{
    // ── Nullable — safe to read on any path (unauthenticated, background jobs, audit) ──
    Guid?   UserId      { get; }
    Guid?   MemberId    { get; }
    Guid?   ClinicId    { get; }
    string? CountryCode { get; }
    string? FullName    { get; }
    string? Username    { get; }
    string? Email       { get; }
    string  IpAddress   { get; }
    string? UserAgent   { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }

    // ── Required — throws UnauthorizedAccessException if the claim is missing ──
    // Use only inside handlers that are behind [Authorize].
    Guid GetRequiredUserId();
    Guid GetRequiredClinicId();
}
