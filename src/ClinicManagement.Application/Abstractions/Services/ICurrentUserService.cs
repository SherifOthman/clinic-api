namespace ClinicManagement.Application.Abstractions.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? ClinicId { get; }
    string? CountryCode { get; }
    string? FullName { get; }
    string? Username { get; }
    string? Email { get; }
    string IpAddress { get; }
    string? UserAgent { get; }
    IEnumerable<string> Roles { get; }
    IEnumerable<string> Permissions { get; }
    bool IsAuthenticated { get; }
    Guid GetRequiredUserId();
    Guid GetRequiredClinicId();
    bool TryGetUserId(out Guid userId);
    bool TryGetClinicId(out Guid clinicId);
}
