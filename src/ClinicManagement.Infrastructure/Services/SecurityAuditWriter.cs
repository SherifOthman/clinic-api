using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using System.Text.Json;

namespace ClinicManagement.Infrastructure.Services;

/// <inheritdoc />
public class SecurityAuditWriter : ISecurityAuditWriter
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public SecurityAuditWriter(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task WriteAsync(
        Guid? userId,
        string? fullName,
        string? username,
        string? userEmail,
        string? userRole,
        Guid? clinicId,
        string eventName,
        string? detail = null,
        CancellationToken cancellationToken = default)
    {
        var changes = new Dictionary<string, string?> { ["event"] = eventName };
        if (!string.IsNullOrEmpty(detail))
            changes["detail"] = detail;

        _context.AuditLogs.Add(new AuditLog
        {
            Timestamp  = DateTime.UtcNow,
            ClinicId   = clinicId,
            UserId     = userId,
            FullName   = fullName,
            Username   = username,
            UserEmail  = userEmail,
            UserRole   = userRole,
            UserAgent  = _currentUserService.UserAgent,
            EntityType = "Auth",
            EntityId   = userId?.ToString() ?? "unknown",
            Action     = AuditAction.Security,
            IpAddress  = _currentUserService.IpAddress,
            Changes    = JsonSerializer.Serialize(changes),
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
