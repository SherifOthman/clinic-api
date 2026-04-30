using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Orchestrates all demo data seeding in the correct order.
/// Each concern is in its own class; this just wires them together.
///
/// Folder: Seeders/Demo/ — separate from system seeders (roles, plans, diseases, geo).
/// </summary>
public class DemoDataOrchestrator
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DemoDataOrchestrator> _logger;
    private readonly SeedOptions _opts;

    public DemoDataOrchestrator(
        ApplicationDbContext db,
        UserManager<User> userManager,
        ILogger<DemoDataOrchestrator> logger,
        IOptions<SeedOptions> opts)
    {
        _db          = db;
        _userManager = userManager;
        _logger      = logger;
        _opts        = opts.Value;
    }

    public async Task SeedAsync()
    {
        if (!_opts.SeedDemoUsers)
        {
            _logger.LogInformation("Demo data seeding disabled");
            return;
        }

        // Skip entirely if demo clinic already exists
        var ownerExists = await _userManager.FindByEmailAsync(_opts.ClinicOwner.Email);
        if (ownerExists is not null)
        {
            var clinicExists = await _db.Set<Clinic>()
                .IgnoreQueryFilters([QueryFilterNames.Tenant])
                .AnyAsync(c => c.OwnerUserId == ownerExists.Id);
            if (clinicExists)
            {
                _logger.LogInformation("Demo data already seeded — checking permissions...");
                // Ensure existing demo users have all current default permissions
                await EnsureDemoPermissionsAsync(ownerExists.Id);
                return;
            }
        }

        _logger.LogInformation("Starting demo data seeding...");

        // 1. Clinic, branches, users, members
        var clinicSeed = new DemoClinicSeed(_db, _userManager,
            _logger.CreateLogger<DemoClinicSeed>(), _opts);
        var clinicResult = await clinicSeed.SeedAsync();
        if (clinicResult is null) return;

        var (clinic, mainBranch, westBranch, ownerDoctor, staffDoctor, doctor3, doctor4, ownerUser) = clinicResult.Value;

        // 2. Schedules and visit types
        var scheduleSeed = new DemoScheduleSeed(_db, _logger.CreateLogger<DemoScheduleSeed>());
        var (ownerSched, staffSched, doc3Sched, doc4Sched,
             ownerVt1, ownerVt2, ownerVt3,
             staffVt1, staffVt2, staffVt3,
             doc3Vt1, doc3Vt2,
             doc4Vt1, doc4Vt2) =
            await scheduleSeed.SeedAsync(ownerDoctor, staffDoctor, doctor3, doctor4, mainBranch);

        // 3. Patients (25 with phones and chronic diseases)
        var patientSeed = new DemoPatientSeed(_db, _logger.CreateLogger<DemoPatientSeed>());
        var patients = await patientSeed.SeedAsync(clinic.Id);

        // 4. Appointments (queue + time, multiple days, all statuses)
        var apptSeed = new DemoAppointmentSeed(_db, _logger.CreateLogger<DemoAppointmentSeed>());
        await apptSeed.SeedAsync(clinic.Id, mainBranch,
            ownerDoctor, staffDoctor, doctor3, doctor4,
            ownerVt1, ownerVt2, ownerVt3,
            staffVt1, staffVt2, staffVt3,
            doc3Vt1, doc3Vt2,
            doc4Vt1, doc4Vt2,
            patients);

        // 5. Staff invitations
        var inviteSeed = new DemoInvitationSeed(_db, _logger.CreateLogger<DemoInvitationSeed>());
        await inviteSeed.SeedAsync(clinic.Id, ownerUser.Id);

        // 6. Testimonials and contact messages
        var contentSeed = new DemoContentSeed(_db, _logger.CreateLogger<DemoContentSeed>());
        await contentSeed.SeedAsync(clinic.Id, ownerUser.Id, clinic.Name);

        _logger.LogInformation("Demo data seeding complete ✓");
    }

    /// <summary>
    /// Adds any missing default permissions to existing demo clinic members,
    /// and creates missing DoctorBranchSchedule entries for demo doctors.
    /// Runs on every startup so new permissions and schedules are automatically
    /// added to demo accounts without a full re-seed.
    /// </summary>
    private async Task EnsureDemoPermissionsAsync(Guid ownerUserId)
    {
        var clinic = await _db.Set<Clinic>()
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .FirstOrDefaultAsync(c => c.OwnerUserId == ownerUserId);
        if (clinic is null) return;

        var branch = await _db.Set<ClinicBranch>()
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .FirstOrDefaultAsync(b => b.ClinicId == clinic.Id && b.IsMainBranch);

        var members = await _db.Set<ClinicMember>()
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .Include(m => m.Permissions)
            .Where(m => m.ClinicId == clinic.Id)
            .ToListAsync();

        int added = 0;
        foreach (var member in members)
        {
            // ── Ensure permissions ────────────────────────────────────────────
            var defaults = DefaultPermissions.ForRole(member.Role);
            var existing = member.Permissions.Select(p => p.Permission).ToHashSet();
            var missing  = defaults.Where(p => !existing.Contains(p)).ToList();
            if (missing.Count > 0)
            {
                _db.Set<ClinicMemberPermission>().AddRange(
                    missing.Select(p => new ClinicMemberPermission { ClinicMemberId = member.Id, Permission = p }));
                added += missing.Count;
            }
        }

        // ── Ensure DoctorBranchSchedule for all doctors ───────────────────────
        if (branch is not null)
        {
            var doctorInfos = await _db.Set<DoctorInfo>()
                .IgnoreQueryFilters([QueryFilterNames.Tenant])
                .Where(d => d.ClinicMember.ClinicId == clinic.Id)
                .ToListAsync();

            foreach (var doctor in doctorInfos)
            {
                var hasSchedule = await _db.Set<DoctorBranchSchedule>()
                    .IgnoreQueryFilters([QueryFilterNames.Tenant])
                    .AnyAsync(s => s.DoctorInfoId == doctor.Id && s.BranchId == branch.Id);

                if (!hasSchedule)
                {
                    _db.Set<DoctorBranchSchedule>().Add(new DoctorBranchSchedule
                    {
                        DoctorInfoId = doctor.Id,
                        BranchId     = branch.Id,
                        IsActive     = true,
                    });
                    _logger.LogInformation("Created missing DoctorBranchSchedule for DoctorInfoId={Id}", doctor.Id);
                    added++;
                }
            }
        }

        if (added > 0)
        {
            await _db.SaveChangesAsync();
            _logger.LogInformation("Fixed {Count} missing item(s) for demo clinic members", added);
        }
    }
}

// Extension to create typed loggers from ILogger<T>
file static class LoggerExtensions
{
    public static ILogger<T> CreateLogger<T>(this ILogger logger)
    {
        // Use the same underlying factory — works because ILogger<T> is just a typed wrapper
        if (logger is ILogger<DemoDataOrchestrator> typed)
        {
            // Can't easily get the factory from ILogger<T>, so use a simple wrapper
        }
        return new TypedLogger<T>(logger);
    }

    private sealed class TypedLogger<T>(ILogger inner) : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => inner.BeginScope(state);
        public bool IsEnabled(LogLevel logLevel) => inner.IsEnabled(logLevel);
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => inner.Log(logLevel, eventId, state, exception, formatter);
    }
}
