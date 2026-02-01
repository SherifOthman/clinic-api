namespace ClinicManagement.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? ClinicId { get; }
    string? Email { get; }
    string IpAddress { get; }
    string? UserAgent { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
    
    // Helper methods that return results instead of throwing exceptions
    Guid GetRequiredUserId();
    Guid GetRequiredClinicId();
    bool TryGetUserId(out Guid userId);
    bool TryGetClinicId(out Guid clinicId);
    bool HasClinicAccess();
}
