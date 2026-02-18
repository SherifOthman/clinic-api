namespace ClinicManagement.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    int? ClinicId { get; }
    string? Email { get; }
    string IpAddress { get; }
    string? UserAgent { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
    int GetRequiredUserId();
    int GetRequiredClinicId();
    bool TryGetUserId(out int userId);
    bool TryGetClinicId(out int clinicId);
}
