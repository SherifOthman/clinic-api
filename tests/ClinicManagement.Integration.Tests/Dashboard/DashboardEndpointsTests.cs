using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClinicManagement.Integration.Tests.Common;
using FluentAssertions;

namespace ClinicManagement.Integration.Tests.Dashboard;

public class DashboardEndpointsTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public DashboardEndpointsTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetDashboardStats_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _factory.CreateClient().GetAsync("/api/dashboard/stats");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDashboardStats_ShouldReturnOk_WhenClinicOwner()
    {
        var token = await ClinicHelper.CreateClinicOwnerAsync(_factory, _client);
        _client.SetBearerToken(token);

        var response = await _client.GetAsync("/api/dashboard/stats");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.TryGetProperty("totalPatients", out _).Should().BeTrue();
        body.TryGetProperty("activeStaff", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetRecentPatients_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _factory.CreateClient().GetAsync("/api/dashboard/recent-patients");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetRecentPatients_ShouldReturnEmptyList_WhenNoPatients()
    {
        var token = await ClinicHelper.CreateClinicOwnerAsync(_factory, _client);
        _client.SetBearerToken(token);

        var response = await _client.GetAsync("/api/dashboard/recent-patients");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task GetRecentPatients_ShouldReturnPatients_AfterCreating()
    {
        var token = await ClinicHelper.CreateClinicOwnerAsync(_factory, _client);
        _client.SetBearerToken(token);

        await _client.PostAsJsonAsync("/api/patients", new
        {
            fullName = "Recent Patient",
            dateOfBirth = "1990-01-01",
            gender = "Male",
            phoneNumbers = new[] { new { phoneNumber = "+966500000600" } },
            chronicDiseaseIds = Array.Empty<Guid>()
        });

        var response = await _client.GetAsync("/api/dashboard/recent-patients");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);

        body.GetArrayLength().Should().BeGreaterThan(0);
        body[0].GetProperty("fullName").GetString().Should().Be("Recent Patient");
        body[0].GetProperty("gender").GetString().Should().Be("Male");
    }
}
