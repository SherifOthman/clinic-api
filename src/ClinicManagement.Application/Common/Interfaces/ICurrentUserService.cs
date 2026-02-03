namespace ClinicManagement.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string IpAddress { get; }
    string? UserAgent { get; }
    IEnumerable<string> Roles { get; }
    bool IsAuthenticated { get; }
    
    // Helper methods
    Guid GetRequiredUserId();
    bool TryGetUserId(out Guid userId);
}
