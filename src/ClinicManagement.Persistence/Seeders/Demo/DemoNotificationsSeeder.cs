using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Seeds 30 notifications for the clinic owner — enough for 3 pages (10/page).
/// Mix of all types (Error, Warning, Success, Info), read/unread states.
/// </summary>
public class DemoNotificationsSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<DemoNotificationsSeeder> _logger;

    public DemoNotificationsSeeder(ApplicationDbContext db, ILogger<DemoNotificationsSeeder> logger)
    {
        _db     = db;
        _logger = logger;
    }

    public async Task SeedAsync(DemoClinicContext ctx)
    {
        var existing = await _db.Set<Notification>()
            .CountAsync(n => n.UserId == ctx.OwnerUserId);

        if (existing >= 20) { _logger.LogInformation("Notifications already seeded — skipping"); return; }

        var now = DateTimeOffset.UtcNow;
        var expiry = now.AddDays(7);

        var notifications = new[]
        {
            // ── Unread — critical (show as badge) ────────────────────────────
            (NotificationType.Error,   "Critical: 98% of monthly patients limit used",
             "You have used 196 of 200 patients this month (98%). You are about to reach your plan limit. Consider upgrading your plan.",
             "/usage", false, (DateTimeOffset?)null, now.AddMinutes(-30)),

            (NotificationType.Error,   "Critical: 96% of monthly appointments limit used",
             "You have used 480 of 500 appointments this month (96%). You are about to reach your plan limit. Consider upgrading your plan.",
             "/usage", false, null, now.AddHours(-2)),

            (NotificationType.Error,   "Critical: 97% of monthly invoices limit used",
             "You have used 194 of 200 invoices this month (97%). You are about to reach your plan limit. Consider upgrading your plan.",
             "/usage", false, null, now.AddHours(-3)),

            // ── Unread — warnings ─────────────────────────────────────────────
            (NotificationType.Warning, "Warning: 85% of monthly patients limit used",
             "You have used 170 of 200 patients this month (85%). You are approaching your plan limit.",
             "/usage", false, null, now.AddHours(-5)),

            (NotificationType.Warning, "Warning: 82% of monthly appointments limit used",
             "You have used 410 of 500 appointments this month (82%). You are approaching your plan limit.",
             "/usage", false, null, now.AddHours(-6)),

            (NotificationType.Warning, "Subscription Expiring Soon",
             "Your subscription for Demo Clinic expires on 2026-06-05 (7 days remaining). Renew to avoid interruption.",
             "/usage", false, null, now.AddHours(-8)),

            // ── Unread — info ─────────────────────────────────────────────────
            (NotificationType.Info,    "New Feature: Appointment Reminders",
             "Automated SMS reminders for appointments are now available. Enable them in settings to reduce no-shows.",
             "/settings", false, null, now.AddHours(-10)),

            (NotificationType.Info,    "System Maintenance Scheduled",
             "Scheduled maintenance on June 10, 2026 from 2:00 AM to 4:00 AM UTC. No action needed.",
             (string?)null, false, null, now.AddHours(-12)),

            // ── Unread — success ──────────────────────────────────────────────
            (NotificationType.Success, "Payment Processed Successfully",
             "Your subscription payment of $199.00 for June 2026 has been processed successfully.",
             (string?)null, false, null, now.AddHours(-14)),

            // ── Read — recent ─────────────────────────────────────────────────
            (NotificationType.Error,   "Critical: 97% of monthly invoices limit used",
             "You have used 194 of 200 invoices this month (97%). You are about to reach your plan limit.",
             "/usage", true, now.AddHours(-20), now.AddDays(-1)),

            (NotificationType.Warning, "Warning: 81% of monthly invoices limit used",
             "You have used 162 of 200 invoices this month (81%). You are approaching your plan limit.",
             "/usage", true, now.AddDays(-1), now.AddDays(-2)),

            (NotificationType.Warning, "Subscription Expiring Soon",
             "Your subscription for Demo Clinic expires on 2026-06-05 (14 days remaining).",
             "/usage", true, now.AddDays(-2), now.AddDays(-3)),

            (NotificationType.Success, "Subscription Upgraded to Professional",
             "Your plan has been upgraded to Professional. Advanced reporting and API access are now available.",
             "/usage", true, now.AddDays(-3), now.AddDays(-4)),

            (NotificationType.Info,    "Monthly Report Ready",
             "Your April 2026 clinic performance report is ready. 312 appointments, 89 new patients.",
             "/dashboard", true, now.AddDays(-4), now.AddDays(-5)),

            (NotificationType.Success, "New Staff Member Joined",
             "Dr. Demo Doctor has accepted the invitation and joined Demo Clinic.",
             "/staff", true, now.AddDays(-5), now.AddDays(-6)),

            (NotificationType.Warning, "Warning: 83% of storage used",
             "You have used 1.66 of 2 GB storage (83%). You are approaching your plan limit.",
             "/usage", true, now.AddDays(-6), now.AddDays(-7)),

            (NotificationType.Info,    "Tip: Bulk Patient Import",
             "You can import multiple patients at once using a CSV file. Visit the patients page to get started.",
             (string?)null, true, now.AddDays(-7), now.AddDays(-8)),

            (NotificationType.Success, "Backup Completed",
             "Your clinic data backup completed successfully. 1.2 GB backed up to secure storage.",
             (string?)null, true, now.AddDays(-8), now.AddDays(-9)),

            (NotificationType.Error,   "Critical: Staff limit reached",
             "You have 3 of 3 active staff members. You cannot add more staff without upgrading your plan.",
             "/usage", true, now.AddDays(-9), now.AddDays(-10)),

            (NotificationType.Warning, "Trial Period Ending in 7 Days",
             "Your trial period ends in 7 days. Upgrade to a paid plan to continue using all features.",
             "/usage", true, now.AddDays(-11), now.AddDays(-12)),

            (NotificationType.Info,    "Security Update Applied",
             "We have enhanced our security measures. All sessions have been refreshed for your protection.",
             (string?)null, true, now.AddDays(-14), now.AddDays(-15)),

            (NotificationType.Success, "Email Verified",
             "Your email address owner@clinic.com has been verified successfully.",
             (string?)null, true, now.AddDays(-17), now.AddDays(-18)),

            (NotificationType.Info,    "Welcome to Demo Clinic!",
             "Your clinic is set up and ready. Start by adding your first patient or inviting staff members.",
             "/dashboard", true, now.AddDays(-19), now.AddDays(-20)),

            (NotificationType.Warning, "Incomplete Profile",
             "Your clinic profile is missing a billing email. Add one in settings to receive invoices.",
             "/settings", true, now.AddDays(-24), now.AddDays(-25)),

            (NotificationType.Success, "Clinic Setup Complete",
             "Congratulations! Your clinic profile is complete. You can now start managing patients and appointments.",
             "/dashboard", true, now.AddDays(-29), now.AddDays(-30)),

            // Extra to push past 3 pages
            (NotificationType.Info,    "New Specialization Added",
             "Cardiology has been added to the available specializations. You can now assign it to your doctors.",
             "/staff", true, now.AddDays(-32), now.AddDays(-33)),

            (NotificationType.Success, "Patient Record Restored",
             "Patient record for Ahmed Hassan (0001) has been successfully restored from archive.",
             "/patients", true, now.AddDays(-35), now.AddDays(-36)),

            (NotificationType.Warning, "Appointment No-Show Rate High",
             "Your no-show rate this month is 18%. Consider enabling SMS reminders to reduce missed appointments.",
             "/settings", true, now.AddDays(-38), now.AddDays(-39)),

            (NotificationType.Info,    "System Update Available",
             "A new version of ClinicCare is available with improved performance and bug fixes. Update will apply automatically.",
             (string?)null, true, now.AddDays(-42), now.AddDays(-43)),

            (NotificationType.Success, "First Appointment Booked",
             "Congratulations on your first appointment booking! Your clinic is now fully operational.",
             "/appointments", true, now.AddDays(-44), now.AddDays(-45)),
        };

        var list = notifications.Select(n => new Notification
        {
            UserId    = ctx.OwnerUserId,
            Type      = n.Item1,
            Title     = n.Item2,
            Message   = n.Item3,
            ActionUrl = n.Item4,
            IsRead    = n.Item5,
            ReadAt    = n.Item6,
            ExpiresAt = expiry,
            CreatedAt = n.Item7,
        }).ToList();

        _db.Set<Notification>().AddRange(list);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} demo notifications", list.Count);
    }
}
