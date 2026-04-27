using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClinicManagement.Integration.Tests.Common;
using FluentAssertions;

namespace ClinicManagement.Integration.Tests.Staff;

public class PermissionsEndpointsTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public PermissionsEndpointsTests(IntegrationTestFactory factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient client, string staffId)> CreateOwnerClientWithStaffAsync()
    {
        var client = _factory.CreateClient();
        var token  = await ClinicHelper.CreateClinicOwnerAsync(_factory, client);
        client.SetBearerToken(token);

        // Invite a receptionist so we have a staff member to manage
        var inviteResponse = await client.PostAsJsonAsync("/api/staff/invite", new
        {
            role  = "Receptionist",
            email = $"perm_{Guid.NewGuid():N}@test.com"
        });
        var inviteBody = await inviteResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var invitationId = inviteBody.GetProperty("invitationId").GetString();

        // Get the staff list — owner is already a member
        var listResponse = await client.GetAsync("/api/staff?pageNumber=1&pageSize=10");
        var listBody     = await listResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var staffId      = listBody.GetProperty("items")[0].GetProperty("id").GetString()!;

        return (client, staffId);
    }

    // ── Auth guards ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPermissions_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _factory.CreateClient().GetAsync($"/api/staff/{Guid.NewGuid()}/permissions");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SetPermissions_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _factory.CreateClient().PutAsJsonAsync(
            $"/api/staff/{Guid.NewGuid()}/permissions", new[] { "ViewPatients" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── With clinic context ───────────────────────────────────────────────────

    [Fact]
    public async Task GetPermissions_ShouldReturn200_WhenOwnerRequestsOwnStaff()
    {
        var (client, staffId) = await CreateOwnerClientWithStaffAsync();

        var response = await client.GetAsync($"/api/staff/{staffId}/permissions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task SetPermissions_ShouldReturn204_WithValidPermissions()
    {
        var (client, staffId) = await CreateOwnerClientWithStaffAsync();

        var response = await client.PutAsJsonAsync($"/api/staff/{staffId}/permissions", new[]
        {
            "ViewPatients",
            "CreatePatient"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SetThenGetPermissions_ShouldReflectChanges()
    {
        var (client, staffId) = await CreateOwnerClientWithStaffAsync();

        await client.PutAsJsonAsync($"/api/staff/{staffId}/permissions", new[]
        {
            "ViewPatients",
            "CreatePatient",
            "EditPatient"
        });

        var getResponse = await client.GetAsync($"/api/staff/{staffId}/permissions");
        var body        = await getResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var permissions = body.EnumerateArray().Select(x => x.GetString()).ToList();

        permissions.Should().Contain("ViewPatients");
        permissions.Should().Contain("CreatePatient");
        permissions.Should().Contain("EditPatient");
    }

    [Fact]
    public async Task SetPermissions_ShouldIgnoreInvalidPermissionNames()
    {
        var (client, staffId) = await CreateOwnerClientWithStaffAsync();

        // "FakePermission" is not a valid Permission enum value — should be silently ignored
        var response = await client.PutAsJsonAsync($"/api/staff/{staffId}/permissions", new[]
        {
            "ViewPatients",
            "FakePermission"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await client.GetAsync($"/api/staff/{staffId}/permissions");
        var body        = await getResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var permissions = body.EnumerateArray().Select(x => x.GetString()).ToList();

        permissions.Should().Contain("ViewPatients");
        permissions.Should().NotContain("FakePermission");
    }
}
