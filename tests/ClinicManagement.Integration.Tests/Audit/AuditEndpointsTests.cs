using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClinicManagement.Integration.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicManagement.Integration.Tests.Audit;

public class AuditEndpointsTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public AuditEndpointsTests(IntegrationTestFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateSuperAdminClientAsync()
    {
        var client = _factory.CreateClient();
        var email  = await AuthHelper.RegisterAsync(client);
        using var scope = _factory.Services.CreateScope();
        var db          = scope.ServiceProvider.GetRequiredService<ClinicManagement.Persistence.ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ClinicManagement.Domain.Entities.User>>();
        var user        = await userManager.FindByEmailAsync(email);
        await userManager.RemoveFromRoleAsync(user!, "ClinicOwner");
        await userManager.AddToRoleAsync(user!, "SuperAdmin");
        var token = await AuthHelper.LoginAsync(client, email);
        client.SetBearerToken(token!);
        return client;
    }

    // ── Auth guards ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAuditLogs_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _factory.CreateClient().GetAsync("/api/audit");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAuditLogs_ShouldReturn403_WhenClinicOwner()
    {
        var client = _factory.CreateClient();
        var token  = await ClinicHelper.CreateClinicOwnerAsync(_factory, client);
        client.SetBearerToken(token);

        var response = await client.GetAsync("/api/audit");
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ── SuperAdmin access ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetAuditLogs_ShouldReturn200_WhenSuperAdmin()
    {
        var client = await CreateSuperAdminClientAsync();

        var response = await client.GetAsync("/api/audit?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.TryGetProperty("items", out _).Should().BeTrue();
        body.TryGetProperty("totalCount", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetAuditLogs_ShouldReturn200_WithEntityTypeFilter()
    {
        var client = await CreateSuperAdminClientAsync();

        var response = await client.GetAsync("/api/audit?entityType=Patient&pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAuditLogs_ShouldReturn200_WithDateRangeFilter()
    {
        var client = await CreateSuperAdminClientAsync();

        var response = await client.GetAsync(
            "/api/audit?from=2026-01-01T00:00:00Z&to=2026-12-31T23:59:59Z&pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
