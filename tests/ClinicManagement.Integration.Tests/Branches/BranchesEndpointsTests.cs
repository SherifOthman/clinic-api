using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClinicManagement.Integration.Tests.Common;
using FluentAssertions;

namespace ClinicManagement.Integration.Tests.Branches;

public class BranchesEndpointsTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public BranchesEndpointsTests(IntegrationTestFactory factory)
    {
        _factory = factory;
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var token = await ClinicHelper.CreateClinicOwnerAsync(_factory, client);
        client.SetBearerToken(token);
        return client;
    }

    [Fact]
    public async Task GetBranches_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _factory.CreateClient().GetAsync("/api/branches");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetBranches_ShouldReturnList_WhenClinicOwner()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/branches");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.GetArrayLength().Should().BeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public async Task CreateBranch_ShouldReturn201_WithValidData()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/branches", new
        {
            name = "North Branch",
            addressLine = "456 North Street",
            stateGeonameId = 2,
            cityGeonameId = 3,
            phoneNumbers = Array.Empty<string>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateBranch_ShouldReturn400_WithMissingFields()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/api/branches", new
        {
            stateGeonameId = 2
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateBranch_ShouldAppearInList()
    {
        var client = await CreateAuthenticatedClientAsync();

        await client.PostAsJsonAsync("/api/branches", new
        {
            name = "South Branch",
            addressLine = "789 South Ave",
            stateGeonameId = 2,
            cityGeonameId = 3,
            phoneNumbers = Array.Empty<string>()
        });

        var listResponse = await client.GetAsync("/api/branches");
        var body = await listResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);

        var names = body.EnumerateArray()
            .Select(b => b.GetProperty("name").GetString())
            .ToList();

        names.Should().Contain("South Branch");
    }

    [Fact]
    public async Task UpdateBranch_ShouldReturn204_WhenValid()
    {
        var client = await CreateAuthenticatedClientAsync();

        var createResponse = await client.PostAsJsonAsync("/api/branches", new
        {
            name = "Branch To Update",
            addressLine = "Old Address",
            stateGeonameId = 2,
            cityGeonameId = 3,
            phoneNumbers = Array.Empty<string>()
        });
        var branchId = (await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts))
            .GetString();

        var updateResponse = await client.PutAsJsonAsync($"/api/branches/{branchId}", new
        {
            name = "Updated Branch",
            addressLine = "New Address",
            stateGeonameId = 2,
            cityGeonameId = 3,
            phoneNumbers = Array.Empty<string>()
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task SetBranchActiveStatus_ShouldReturn204_WhenValid()
    {
        var client = await CreateAuthenticatedClientAsync();

        var createResponse = await client.PostAsJsonAsync("/api/branches", new
        {
            name = "Branch To Deactivate",
            addressLine = "Some Address",
            stateGeonameId = 2,
            cityGeonameId = 3,
            phoneNumbers = Array.Empty<string>()
        });
        var branchId = (await createResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts))
            .GetString();

        var response = await client.PatchAsJsonAsync($"/api/branches/{branchId}/active-status", new
        {
            isActive = false
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
