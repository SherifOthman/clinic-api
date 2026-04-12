using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace ClinicManagement.Infrastructure.Services;

public class SecurityAuditWriter : ISecurityAuditWriter
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly DbSet<AuditLog> _auditLogs;

    public SecurityAuditWriter(ApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context            = context;
        _currentUserService = currentUserService;
        _auditLogs          = context.Set<AuditLog>();
    }

    public async Task WriteAsync(
        Guid? userId, string? fullName, string? username, string? userEmail,
        string? userRole, Guid? clinicId, string eventName,
        string? detail = null, CancellationToken cancellationToken = default)
    {
        var changes = new Dictionary<string, string?> { ["event"] = eventName };
        if (!string.IsNullOrEmpty(detail))
            changes["detail"] = detail;

        _auditLogs.Add(new AuditLog
        {
            Timestamp  = DateTimeOffset.UtcNow,
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
