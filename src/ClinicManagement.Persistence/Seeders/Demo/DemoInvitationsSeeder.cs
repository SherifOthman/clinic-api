using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Seeds staff invitations in various states: pending, accepted, expired, cancelled.
/// Gives the invitations page something to show.
/// </summary>
public class DemoInvitationsSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DemoInvitationsSeeder> _logger;

    public DemoInvitationsSeeder(ApplicationDbContext db, ILogger<DemoInvitationsSeeder> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task SeedAsync(DemoClinicContext ctx)
    {
        var existing = await _db.Set<StaffInvitation>().IgnoreQueryFilters()
            .CountAsync(i => i.ClinicId == ctx.ClinicId);

        if (existing >= 5) { _logger.LogInformation("Invitations already seeded — skipping"); return; }

        var now  = DateTimeOffset.UtcNow;
        var spec = await _db.Set<Specialization>().FirstOrDefaultAsync(s => s.NameEn == "Cardiology");

        var invitations = new[]
        {
            // Pending — valid, expires in future
            StaffInvitation.Create(ctx.ClinicId, "dr.ahmed.new@clinic.com",    ClinicMemberRole.Doctor,       ctx.OwnerUserId, spec?.Id),
            StaffInvitation.Create(ctx.ClinicId, "receptionist2@clinic.com",   ClinicMemberRole.Receptionist, ctx.OwnerUserId),
            StaffInvitation.Create(ctx.ClinicId, "dr.sara.pending@clinic.com", ClinicMemberRole.Doctor,       ctx.OwnerUserId),
        };

        // Expired invitation
        var expired = StaffInvitation.Create(ctx.ClinicId, "expired.doctor@clinic.com", ClinicMemberRole.Doctor, ctx.OwnerUserId);
        expired.ExpiresAt = now.AddDays(-3); // already expired

        // Cancelled invitation
        var cancelled = StaffInvitation.Create(ctx.ClinicId, "cancelled@clinic.com", ClinicMemberRole.Receptionist, ctx.OwnerUserId);
        cancelled.Cancel();

        foreach (var inv in invitations)
        {
            inv.CreatedAt  = now.AddDays(-Random.Shared.Next(1, 5));
            inv.UpdatedAt  = inv.CreatedAt;
            inv.CreatedBy  = ctx.OwnerUserId;
            inv.UpdatedBy  = ctx.OwnerUserId;
            _db.Set<StaffInvitation>().Add(inv);
        }

        expired.CreatedAt  = now.AddDays(-10);
        expired.UpdatedAt  = now.AddDays(-10);
        expired.CreatedBy  = ctx.OwnerUserId;
        expired.UpdatedBy  = ctx.OwnerUserId;
        _db.Set<StaffInvitation>().Add(expired);

        cancelled.CreatedAt = now.AddDays(-7);
        cancelled.UpdatedAt = now.AddDays(-6);
        cancelled.CreatedBy = ctx.OwnerUserId;
        cancelled.UpdatedBy = ctx.OwnerUserId;
        _db.Set<StaffInvitation>().Add(cancelled);

        await _db.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} demo invitations", invitations.Length + 2);
    }
}
