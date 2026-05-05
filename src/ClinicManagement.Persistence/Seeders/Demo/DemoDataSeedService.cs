using Microsoft.Extensions.Logging;

namespace ClinicManagement.Persistence.Seeders.Demo;

/// <summary>
/// Orchestrates all demo data seeding.
/// Only runs when ASPNETCORE_ENVIRONMENT is Development or Staging.
/// Each sub-seeder is idempotent — safe to call multiple times.
/// </summary>
public class DemoDataSeedService
{
    private readonly DemoClinicSeeder       _clinicSeeder;
    private readonly DemoPatientsSeeder     _patientsSeeder;
    private readonly DemoAppointmentsSeeder _appointmentsSeeder;
    private readonly DemoContactSeeder      _contactSeeder;
    private readonly DemoTestimonialsSeeder _testimonialsSeeder;
    private readonly DemoNotificationsSeeder _notificationsSeeder;
    private readonly DemoAuditSeeder        _auditSeeder;
    private readonly DemoInvitationsSeeder  _invitationsSeeder;
    private readonly DemoUsageMetricsSeeder _usageMetricsSeeder;
    private readonly ILogger<DemoDataSeedService> _logger;

    public DemoDataSeedService(
        DemoClinicSeeder       clinicSeeder,
        DemoPatientsSeeder     patientsSeeder,
        DemoAppointmentsSeeder appointmentsSeeder,
        DemoContactSeeder      contactSeeder,
        DemoTestimonialsSeeder testimonialsSeeder,
        DemoNotificationsSeeder notificationsSeeder,
        DemoAuditSeeder        auditSeeder,
        DemoInvitationsSeeder  invitationsSeeder,
        DemoUsageMetricsSeeder usageMetricsSeeder,
        ILogger<DemoDataSeedService> logger)
    {
        _clinicSeeder        = clinicSeeder;
        _patientsSeeder      = patientsSeeder;
        _appointmentsSeeder  = appointmentsSeeder;
        _contactSeeder       = contactSeeder;
        _testimonialsSeeder  = testimonialsSeeder;
        _notificationsSeeder = notificationsSeeder;
        _auditSeeder         = auditSeeder;
        _invitationsSeeder   = invitationsSeeder;
        _usageMetricsSeeder  = usageMetricsSeeder;
        _logger              = logger;
    }

    public async Task SeedAsync()
    {
        _logger.LogInformation("Demo data seeding started...");

        var clinicContext = await _clinicSeeder.SeedAsync();
        if (clinicContext is null)
        {
            _logger.LogWarning("Demo clinic context not available — skipping remaining demo seeds");
            return;
        }

        await _patientsSeeder.SeedAsync(clinicContext);
        await _appointmentsSeeder.SeedAsync(clinicContext);
        await _contactSeeder.SeedAsync();
        await _testimonialsSeeder.SeedAsync(clinicContext);
        await _notificationsSeeder.SeedAsync(clinicContext);
        await _auditSeeder.SeedAsync(clinicContext);
        await _invitationsSeeder.SeedAsync(clinicContext);
        await _usageMetricsSeeder.SeedAsync(clinicContext);

        _logger.LogInformation("Demo data seeding completed");
    }
}
