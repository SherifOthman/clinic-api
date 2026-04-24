using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClinicManagement.Integration.Tests.Common;
using FluentAssertions;

namespace ClinicManagement.Integration.Tests.Staff;

public class StaffEndpointsTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public StaffEndpointsTests(IntegrationTestFactory factory)
    {
        _factory = factory;
    }

    // Creates a fresh authenticated client for each test to avoid token bleed
    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var token = await ClinicHelper.CreateClinicOwnerAsync(_factory, client);
        client.SetBearerToken(token);
        return client;
    }

    // ── Auth guards ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStaff_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _factory.CreateClient().GetAsync("/api/staff");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task InviteStaff_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/api/staff/invite", new
        {
            role = "Doctor",
            email = "doctor@test.com"
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetInvitations_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _factory.CreateClient().GetAsync("/api/staff/invitations");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── With clinic context ───────────────────────────────────────────────────

    [Fact]
    public async Task GetStaff_ShouldReturnOk_WhenClinicOwner()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/staff?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task InviteStaff_ShouldReturn200_WithValidReceptionistInvite()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/staff/invite", new
        {
            role = "Receptionist",
            email = $"newrec_{Guid.NewGuid():N}@test.com"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var respBody = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        respBody.TryGetProperty("invitationId", out _).Should().BeTrue();
        respBody.TryGetProperty("token", out _).Should().BeTrue();
    }

    [Fact]
    public async Task InviteStaff_ShouldReturn400_WithInvalidRole()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/staff/invite", new
        {
            role = "SuperAdmin", // not allowed
            email = "hacker@test.com"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetInvitations_ShouldReturnList_AfterInviting()
    {
        var client = await CreateAuthenticatedClientAsync();

        await client.PostAsJsonAsync("/api/staff/invite", new
        {
            role = "Receptionist",
            email = $"rec_{Guid.NewGuid():N}@test.com"
        });

        var response = await client.GetAsync("/api/staff/invitations?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.GetProperty("totalCount").GetInt32().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task CancelInvitation_ShouldReturn204_WhenValid()
    {
        var client = await CreateAuthenticatedClientAsync();

        var inviteResponse = await client.PostAsJsonAsync("/api/staff/invite", new
        {
            role = "Receptionist",
            email = $"cancel_{Guid.NewGuid():N}@test.com"
        });
        inviteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var inviteBody = await inviteResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var invitationId = inviteBody.GetProperty("invitationId").GetString();

        var cancelResponse = await client.DeleteAsync($"/api/staff/invitations/{invitationId}");
        cancelResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetStaffDetail_ShouldReturnDetail_WhenExists()
    {
        var client = await CreateAuthenticatedClientAsync();

        var listResponse = await client.GetAsync("/api/staff?pageNumber=1&pageSize=10");
        var listBody = await listResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var staffId = listBody.GetProperty("items")[0].GetProperty("id").GetString();

        var detailResponse = await client.GetAsync($"/api/staff/{staffId}");

        detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await detailResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        detail.TryGetProperty("fullName", out _).Should().BeTrue();
        detail.TryGetProperty("gender", out _).Should().BeTrue();
    }

    [Fact]
    public async Task SetStaffActiveStatus_ShouldReturn204_WhenValid()
    {
        var client = await CreateAuthenticatedClientAsync();

        var listResponse = await client.GetAsync("/api/staff?pageNumber=1&pageSize=10");
        var listBody = await listResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var staffId = listBody.GetProperty("items")[0].GetProperty("id").GetString();

        var response = await client.PatchAsJsonAsync($"/api/staff/{staffId}/active-status", new
        {
            isActive = false
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
