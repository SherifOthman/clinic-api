using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Seeds 40 audit log entries — enough for 4 pages (10/page).
/// Mix of all action types across different entities.
/// </summary>
public class DemoAuditSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DemoAuditSeeder> _logger;

    public DemoAuditSeeder(ApplicationDbContext db, ILogger<DemoAuditSeeder> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task SeedAsync(DemoClinicContext ctx)
    {
        var existing = await _db.Set<AuditLog>().CountAsync();
        if (existing >= 30) { _logger.LogInformation("Audit logs already seeded — skipping"); return; }

        var now = DateTimeOffset.UtcNow;

        // Get some patient IDs for realistic entity IDs
        var patientIds = await _db.Set<Patient>().IgnoreQueryFilters()
            .Where(p => p.ClinicId == ctx.ClinicId)
            .Select(p => p.Id.ToString())
            .Take(10)
            .ToListAsync();

        var doctorUserId = await _db.Users
            .Where(u => u.Email == "doctor@clinic.com")
            .Select(u => u.Id)
            .FirstOrDefaultAsync();

        var receptionistUserId = await _db.Users
            .Where(u => u.Email == "receptionist@clinic.com")
            .Select(u => u.Id)
            .FirstOrDefaultAsync();

        var entries = new List<AuditLog>();

        // ── Patient operations ────────────────────────────────────────────────
        for (int i = 0; i < 10; i++)
        {
            var entityId = patientIds.Count > i ? patientIds[i] : Guid.NewGuid().ToString();
            entries.Add(new AuditLog
            {
                ClinicId   = ctx.ClinicId,
                UserId     = i % 3 == 0 ? ctx.OwnerUserId : (i % 3 == 1 ? doctorUserId : receptionistUserId),
                FullName   = i % 3 == 0 ? "Clinic Owner" : (i % 3 == 1 ? "Demo Doctor" : "Demo Receptionist"),
                Username   = i % 3 == 0 ? "owner" : (i % 3 == 1 ? "doctor" : "receptionist"),
                UserEmail  = i % 3 == 0 ? "owner@clinic.com" : (i % 3 == 1 ? "doctor@clinic.com" : "receptionist@clinic.com"),
                UserRole   = i % 3 == 0 ? "ClinicOwner" : (i % 3 == 1 ? "Doctor" : "Receptionist"),
                EntityType = "Patient",
                EntityId   = entityId,
                Action     = i < 7 ? AuditAction.Create : (i < 9 ? AuditAction.Update : AuditAction.Delete),
                IpAddress  = $"192.168.1.{10 + i}",
                UserAgent  = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/124.0",
                Changes    = i < 7 ? null : """{"FullName":{"Old":"Old Name","New":"New Name"}}""",
                Timestamp  = now.AddDays(-i).AddHours(-i),
            });
        }

        // ── Appointment operations ────────────────────────────────────────────
        for (int i = 0; i < 10; i++)
        {
            entries.Add(new AuditLog
            {
                ClinicId   = ctx.ClinicId,
                UserId     = i % 2 == 0 ? receptionistUserId : ctx.OwnerUserId,
                FullName   = i % 2 == 0 ? "Demo Receptionist" : "Clinic Owner",
                Username   = i % 2 == 0 ? "receptionist" : "owner",
                UserEmail  = i % 2 == 0 ? "receptionist@clinic.com" : "owner@clinic.com",
                UserRole   = i % 2 == 0 ? "Receptionist" : "ClinicOwner",
                EntityType = "Appointment",
                EntityId   = Guid.NewGuid().ToString(),
                Action     = i < 6 ? AuditAction.Create : AuditAction.Update,
                IpAddress  = $"192.168.1.{20 + i}",
                UserAgent  = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/124.0",
                Changes    = i >= 6 ? """{"Status":{"Old":"Pending","New":"Completed"}}""" : null,
                Timestamp  = now.AddDays(-i - 1).AddHours(-i * 2),
            });
        }

        // ── Staff operations ──────────────────────────────────────────────────
        for (int i = 0; i < 8; i++)
        {
            entries.Add(new AuditLog
            {
                ClinicId   = ctx.ClinicId,
                UserId     = ctx.OwnerUserId,
                FullName   = "Clinic Owner",
                Username   = "owner",
                UserEmail  = "owner@clinic.com",
                UserRole   = "ClinicOwner",
                EntityType = "ClinicMember",
                EntityId   = Guid.NewGuid().ToString(),
                Action     = i < 4 ? AuditAction.Create : AuditAction.Update,
                IpAddress  = "192.168.1.100",
                UserAgent  = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) Safari/537.36",
                Changes    = i >= 4 ? """{"IsActive":{"Old":"true","New":"false"}}""" : null,
                Timestamp  = now.AddDays(-i - 5).AddHours(-3),
            });
        }

        // ── Security events ───────────────────────────────────────────────────
        var securityUsers = new[]
        {
            (ctx.OwnerUserId,       "Clinic Owner",       "owner",        "owner@clinic.com",        "ClinicOwner"),
            (doctorUserId,          "Demo Doctor",        "doctor",       "doctor@clinic.com",        "Doctor"),
            (receptionistUserId,    "Demo Receptionist",  "receptionist", "receptionist@clinic.com",  "Receptionist"),
        };

        for (int i = 0; i < 6; i++)
        {
            var (uid, name, uname, email, role) = securityUsers[i % 3];
            entries.Add(new AuditLog
            {
                ClinicId   = ctx.ClinicId,
                UserId     = uid,
                FullName   = name,
                Username   = uname,
                UserEmail  = email,
                UserRole   = role,
                EntityType = "User",
                EntityId   = uid.ToString(),
                Action     = AuditAction.Security,
                IpAddress  = $"10.0.0.{i + 1}",
                UserAgent  = "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0) Mobile Safari/604.1",
                Changes    = i % 2 == 0 ? """{"Event":"Login","Method":"Password"}""" : """{"Event":"PasswordChanged"}""",
                Timestamp  = now.AddDays(-i - 10).AddHours(-i),
            });
        }

        // ── Restore operations ────────────────────────────────────────────────
        for (int i = 0; i < 6; i++)
        {
            entries.Add(new AuditLog
            {
                ClinicId   = ctx.ClinicId,
                UserId     = ctx.OwnerUserId,
                FullName   = "Clinic Owner",
                Username   = "owner",
                UserEmail  = "owner@clinic.com",
                UserRole   = "ClinicOwner",
                EntityType = i < 3 ? "Patient" : "ClinicMember",
                EntityId   = Guid.NewGuid().ToString(),
                Action     = AuditAction.Restore,
                IpAddress  = "192.168.1.100",
                UserAgent  = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/124.0",
                Changes    = """{"IsDeleted":{"Old":"true","New":"false"}}""",
                Timestamp  = now.AddDays(-i - 15).AddHours(-2),
            });
        }

        _db.Set<AuditLog>().AddRange(entries);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} demo audit log entries", entries.Count);
    }
}
