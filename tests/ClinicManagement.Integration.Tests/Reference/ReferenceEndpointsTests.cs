using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClinicManagement.Integration.Tests.Common;
using FluentAssertions;

namespace ClinicManagement.Integration.Tests.Reference;

public class ReferenceEndpointsTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public ReferenceEndpointsTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSpecializations_ShouldReturn200_WithoutAuth()
    {
        var response = await _factory.CreateClient().GetAsync("/api/specializations");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetSubscriptionPlans_ShouldReturn200_WithoutAuth()
    {
        var response = await _factory.CreateClient().GetAsync("/api/subscription-plans");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetChronicDiseases_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _factory.CreateClient().GetAsync("/api/chronic-diseases");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetChronicDiseases_ShouldReturnOk_WhenClinicOwner()
    {
        // Arrange
        var token = await ClinicHelper.CreateClinicOwnerAsync(_factory, _client);
        _client.SetBearerToken(token);

        // Act
        var response = await _client.GetAsync("/api/chronic-diseases");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.ValueKind.Should().Be(JsonValueKind.Array);
    }
}
