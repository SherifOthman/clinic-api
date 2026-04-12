using ClinicManagement.Application.Abstractions.Email;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Persistence;
using ClinicManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace ClinicManagement.Integration.Tests.Common;

/// <summary>
/// Spins up the full ASP.NET Core pipeline against a real SQL Server LocalDB database.
/// Each factory instance gets its own isolated DB, runs all EF migrations, seeds roles,
/// then drops the DB after the test class completes.
/// </summary>
public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _dbName = $"ClinicTest_{Guid.NewGuid():N}";

    private string ConnectionString =>
        $"Server=(localdb)\\mssqllocaldb;Database={_dbName};Trusted_Connection=True;TrustServerCertificate=True;";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace SQL Server DbContext with isolated test DB
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Remove EF10-style IDbContextOptionsConfiguration registrations
            services
                .Where(d => d.ServiceType.IsGenericType &&
                            d.ServiceType.GetGenericTypeDefinition().FullName?
                                .Contains("IDbContextOptionsConfiguration") == true)
                .ToList()
                .ForEach(d => services.Remove(d));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(ConnectionString));

            // Replace email services with no-ops — tests don't need SMTP
            services.RemoveAll<IEmailService>();
            services.RemoveAll<IEmailTokenService>();
            services.RemoveAll<SmtpEmailSender>();
            services.AddScoped<IEmailService, NoOpEmailService>();
            services.AddScoped<IEmailTokenService, NoOpEmailTokenService>();
            services.AddScoped<SmtpEmailSender, NoOpSmtpEmailSender>();

            // Remove background services — they would connect to the test DB on a timer
            services
                .Where(d => d.ServiceType == typeof(IHostedService))
                .ToList()
                .ForEach(d => services.Remove(d));
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        foreach (var role in new[] { "SuperAdmin", "ClinicOwner", "Doctor", "Receptionist" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new Role { Name = role });
        }
    }

    public new async Task DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureDeletedAsync();
        await base.DisposeAsync();
    }
}

// ── No-op email implementations ───────────────────────────────────────────────

internal sealed class NoOpEmailService : IEmailService
{
    public Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink, CancellationToken ct = default) => Task.CompletedTask;
    public Task SendStaffInvitationEmailAsync(string toEmail, string clinicName, string role, string invitedBy, string invitationLink, CancellationToken ct = default) => Task.CompletedTask;
    public Task SendEmailAsync(string toEmail, string? toName, string subject, string body, bool isHtml = true, CancellationToken ct = default) => Task.CompletedTask;
}

internal sealed class NoOpEmailTokenService : IEmailTokenService
{
    public Task SendConfirmationEmailAsync(User user, CancellationToken ct = default) => Task.CompletedTask;
    public Task ConfirmEmailAsync(User user, string token, CancellationToken ct = default) => Task.CompletedTask;
    public Task<bool> IsEmailConfirmedAsync(User user, CancellationToken ct = default) => Task.FromResult(true);
    public string GeneratePasswordResetToken(Guid userId, string email, string passwordHash) => "test-token";
    public bool ValidatePasswordResetToken(Guid userId, string email, string passwordHash, string token) => true;
}

// SmtpEmailSender is a concrete class injected into EmailService/EmailTokenService.
// We replace it so no SMTP connection is attempted even if the no-op services are bypassed.
internal sealed class NoOpSmtpEmailSender : SmtpEmailSender
{
    public NoOpSmtpEmailSender() : base(
        Microsoft.Extensions.Options.Options.Create(new ClinicManagement.Infrastructure.Options.SmtpOptions()),
        Microsoft.Extensions.Options.Options.Create(new ClinicManagement.Application.Common.Options.AppOptions()),
        Microsoft.Extensions.Logging.Abstractions.NullLogger<SmtpEmailSender>.Instance) { }

    public new Task SendEmailAsync(string toEmail, string subject, string htmlMessage, CancellationToken ct = default)
        => Task.CompletedTask;
}
