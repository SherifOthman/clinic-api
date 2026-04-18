using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClinicManagement.Integration.Tests.Common;
using FluentAssertions;

namespace ClinicManagement.Integration.Tests.Patients;

public class PatientsEndpointsTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public PatientsEndpointsTests(IntegrationTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    // ── Auth guards ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetPatients_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _factory.CreateClient().GetAsync("/api/patients");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePatient_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/api/patients", new
        {
            firstName = "Test", lastName = "User", dateOfBirth = "1990-01-01", gender = "Male",
            phoneNumbers = new[] { "+966500000200" },
            chronicDiseaseIds = Array.Empty<Guid>()
        });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── With clinic context ───────────────────────────────────────────────────

    [Fact]
    public async Task GetPatients_ShouldReturnOk_WhenClinicOwner()
    {
        var token = await ClinicHelper.CreateClinicOwnerAsync(_factory, _client);
        _client.SetBearerToken(token);

        var response = await _client.GetAsync("/api/patients?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.GetProperty("items").GetArrayLength().Should().Be(0);
        body.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task CreatePatient_ShouldReturn201_WithValidData()
    {
        var token = await ClinicHelper.CreateClinicOwnerAsync(_factory, _client);
        _client.SetBearerToken(token);

        var response = await _client.PostAsJsonAsync("/api/patients", new
        {
            firstName = "Ahmed",
            lastName = "Ali",
            dateOfBirth = "1990-06-15",
            gender = "Male",
            phoneNumbers = new[] { "+966500000300" },
            chronicDiseaseIds = Array.Empty<Guid>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreatePatient_ShouldReturn400_WithMissingFields()
    {
        var token = await ClinicHelper.CreateClinicOwnerAsync(_factory, _client);
        _client.SetBearerToken(token);

        var response = await _client.PostAsJsonAsync("/api/patients", new
        {
            // missing firstName, lastName, dateOfBirth, gender
            phoneNumbers = Array.Empty<object>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateThenGetPatient_ShouldAppearInList()
    {
        var token = await ClinicHelper.CreateClinicOwnerAsync(_factory, _client);
        _client.SetBearerToken(token);

        await _client.PostAsJsonAsync("/api/patients", new
        {
            firstName = "Sara",
            lastName = "Mohamed",
            dateOfBirth = "1985-03-20",
            gender = "Female",
            phoneNumbers = new[] { "+966500000400" },
            chronicDiseaseIds = Array.Empty<Guid>()
        });

        var listResponse = await _client.GetAsync("/api/patients?pageNumber=1&pageSize=10");
        var body = await listResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);

        body.GetProperty("totalCount").GetInt32().Should().BeGreaterThan(0);
        var items = body.GetProperty("items");
        items[0].GetProperty("fullName").GetString().Should().Be("Sara Mohamed");
        items[0].GetProperty("gender").GetString().Should().Be("Female");
    }

    [Fact]
    public async Task GetPatientDetail_ShouldReturn400_WhenNotFound()
    {
        var token = await ClinicHelper.CreateClinicOwnerAsync(_factory, _client);
        _client.SetBearerToken(token);

        var response = await _client.GetAsync($"/api/patients/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeletePatient_ShouldReturn204_WhenExists()
    {
        var token = await ClinicHelper.CreateClinicOwnerAsync(_factory, _client);
        _client.SetBearerToken(token);

        await _client.PostAsJsonAsync("/api/patients", new
        {
            firstName = "To",
            lastName = "Delete",
            dateOfBirth = "1990-01-01",
            gender = "Male",
            phoneNumbers = new[] { "+966500000500" },
            chronicDiseaseIds = Array.Empty<Guid>()
        });

        var list = await _client.GetAsync("/api/patients?pageNumber=1&pageSize=10");
        var body = await list.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var id = body.GetProperty("items")[0].GetProperty("id").GetString();

        var deleteResponse = await _client.DeleteAsync($"/api/patients/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterDelete = await _client.GetAsync("/api/patients?pageNumber=1&pageSize=10");
        var afterBody = await afterDelete.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        afterBody.GetProperty("totalCount").GetInt32().Should().Be(0);
    }

    [Fact]
    public async Task UpdatePatient_ShouldReturn204_WhenValid()
    {
        var token = await ClinicHelper.CreateClinicOwnerAsync(_factory, _client);
        _client.SetBearerToken(token);

        await _client.PostAsJsonAsync("/api/patients", new
        {
            firstName = "Original",
            lastName = "Name",
            dateOfBirth = "1990-01-01",
            gender = "Male",
            phoneNumbers = new[] { "+966500000700" },
            chronicDiseaseIds = Array.Empty<Guid>()
        });

        var list = await _client.GetAsync("/api/patients?pageNumber=1&pageSize=10");
        var body = await list.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var id = body.GetProperty("items")[0].GetProperty("id").GetString();

        var updateResponse = await _client.PutAsJsonAsync($"/api/patients/{id}", new
        {
            firstName = "Updated",
            lastName = "Name",
            dateOfBirth = "1990-01-01",
            gender = "Female",
            phoneNumbers = new[] { "+966500000800" },
            chronicDiseaseIds = Array.Empty<Guid>()
        });

        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var detail = await _client.GetAsync($"/api/patients/{id}");
        var detailBody = await detail.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        detailBody.GetProperty("fullName").GetString().Should().Be("Updated Name");
        detailBody.GetProperty("gender").GetString().Should().Be("Female");
    }

    [Fact]
    public async Task GetPatientDetail_ShouldReturnDetail_WhenExists()
    {
        var token = await ClinicHelper.CreateClinicOwnerAsync(_factory, _client);
        _client.SetBearerToken(token);

        await _client.PostAsJsonAsync("/api/patients", new
        {
            firstName = "Detail",
            lastName = "Patient",
            dateOfBirth = "1985-06-15",
            gender = "Female",
            phoneNumbers = new[] { "+966500000900" },
            chronicDiseaseIds = Array.Empty<Guid>()
        });

        var list = await _client.GetAsync("/api/patients?pageNumber=1&pageSize=10");
        var listBody = await list.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        var id = listBody.GetProperty("items")[0].GetProperty("id").GetString();

        var response = await _client.GetAsync($"/api/patients/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.GetProperty("fullName").GetString().Should().Be("Detail Patient");
        body.GetProperty("gender").GetString().Should().Be("Female");
        body.TryGetProperty("phoneNumbers", out _).Should().BeTrue();
    }
}
