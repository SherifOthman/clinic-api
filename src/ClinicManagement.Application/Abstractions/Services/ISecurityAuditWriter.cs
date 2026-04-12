namespace ClinicManagement.Application.Abstractions.Services;


public interface ISecurityAuditWriter
{
    Task WriteAsync(
        Guid? userId,
        string? fullName,
        string? username,
        string? userEmail,
        string? userRole,
        Guid? clinicId,
        string eventName,
        string? detail = null,
        CancellationToken cancellationToken = default);
}
