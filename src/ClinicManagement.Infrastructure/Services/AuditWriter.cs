using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using System.Text.Json;

namespace ClinicManagement.Infrastructure.Services;

/// <summary>
/// Writes business-event audit entries (login, logout, password change, etc.).
/// Entity data-change entries are written by the SaveChanges interceptor in
/// ApplicationDbContext — this class handles only the manual/event path.
/// </summary>
public class AuditWriter : IAuditWriter
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public AuditWriter(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task WriteEventAsync(
        string eventName,
        string? detail = null,
        Guid? overrideUserId = null,
        string? overrideFullName = null,
        string? overrideEmail = null,
        string? overrideRole = null,
        Guid? overrideClinicId = null,
        CancellationToken ct = default)
    {
        // Build the Changes JSON: { "event": "LoginFailed", "detail": "..." }
        var payload = new Dictionary<string, string?> { ["event"] = eventName };
        if (!string.IsNullOrEmpty(detail))
            payload["detail"] = detail;

        await _uow.AuditLogs.AddAsync(new AuditLog
        {
            Timestamp  = DateTimeOffset.UtcNow,
            ClinicId   = overrideClinicId  ?? _currentUser.ClinicId,
            UserId     = overrideUserId    ?? _currentUser.UserId,
            FullName   = overrideFullName  ?? _currentUser.FullName,
            Username   = _currentUser.Username,
            UserEmail  = overrideEmail     ?? _currentUser.Email,
            UserRole   = overrideRole      ?? _currentUser.Roles.FirstOrDefault(),
            UserAgent  = _currentUser.UserAgent,
            IpAddress  = _currentUser.IpAddress,
            EntityType = "Event",
            EntityId   = (overrideUserId ?? _currentUser.UserId)?.ToString() ?? "system",
            Action     = AuditAction.Security,
            Changes    = JsonSerializer.Serialize(payload),
        }, ct);

        await _uow.SaveChangesAsync(ct);
    }
}
