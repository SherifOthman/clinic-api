namespace ClinicManagement.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    int? ClinicId { get; }
    string? Email { get; }
    IEnumerable< string> Roles { get; }
    bool IsAuthenticated { get; }
    
    // Helper methods
    int GetRequiredUserId();
    int GetRequiredClinicId();
    void EnsureAuthenticated();
    void EnsureClinicAccess();
}