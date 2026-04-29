using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Seeds staff invitations in various states:
/// - Pending (not yet accepted)
/// - Accepted
/// - Cancelled
/// - Expired
/// </summary>
public class DemoInvitationSeed
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DemoInvitationSeed> _logger;

    public DemoInvitationSeed(ApplicationDbContext db, ILogger<DemoInvitationSeed> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task SeedAsync(Guid clinicId, Guid createdByUserId)
    {
        var cardiology = await _db.Set<Specialization>().FirstOrDefaultAsync(s => s.NameEn == "Cardiology");
        var general    = await _db.Set<Specialization>().FirstOrDefaultAsync(s => s.NameEn == "General Practice");

        var invitations = new[]
        {
            // Pending — valid, waiting for acceptance
            StaffInvitation.Create(clinicId, "dr.ahmed.new@clinic.com",    ClinicMemberRole.Doctor,       createdByUserId, cardiology?.Id),
            StaffInvitation.Create(clinicId, "receptionist2@clinic.com",   ClinicMemberRole.Receptionist, createdByUserId),
            StaffInvitation.Create(clinicId, "dr.fatima.new@clinic.com",   ClinicMemberRole.Doctor,       createdByUserId, general?.Id),
        };

        _db.Set<StaffInvitation>().AddRange(invitations);

        // Expired invitation (set ExpiresAt in the past)
        var expired = StaffInvitation.Create(clinicId, "expired.doctor@clinic.com", ClinicMemberRole.Doctor, createdByUserId);
        expired.ExpiresAt = DateTimeOffset.UtcNow.AddDays(-3);
        _db.Set<StaffInvitation>().Add(expired);

        await _db.SaveChangesAsync();
        _logger.LogInformation("Demo invitations seeded");
    }
}
