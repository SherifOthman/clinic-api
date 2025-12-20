namespace ClinicManagement.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    int? ClinicId { get; }  // For tenant isolation
    string? Email { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
    
    // Helper methods
    int GetUserId();
    int? GetClinicId();
}
