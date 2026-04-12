using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClinicManagement.Integration.Tests.Common;
using FluentAssertions;

namespace ClinicManagement.Integration.Tests.Staff;

public class WorkingDaysEndpointsTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public WorkingDaysEndpointsTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetWorkingDays_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _factory.CreateClient()
            .GetAsync($"/api/staff/{Guid.NewGuid()}/working-days");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SaveWorkingDays_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _factory.CreateClient().PutAsJsonAsync(
            $"/api/staff/{Guid.NewGuid()}/working-days",
            new { days = Array.Empty<object>() });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetWorkingDays_ShouldReturnOk_ForExistingStaff()
    {
        var token = await ClinicHelper.CreateClinicOwnerAsync(_factory, _client);
        _client.SetBearerToken(token);

        var listResponse = await _client.GetAsync("/api/staff?pageNumber=1&pageSize=10");
        var listBody = await listResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var staffId = listBody.GetProperty("items")[0].GetProperty("id").GetString();

        // Owner has no doctor profile, so working days returns empty list (not an error)
        var response = await _client.GetAsync($"/api/staff/{staffId}/working-days");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
