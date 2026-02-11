using System.Net;
using System.Net.Http.Json;
using ClinicManagement.IntegrationTests.Helpers;
using FluentAssertions;

namespace ClinicManagement.IntegrationTests.Patients;

public class PatientCrudTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public PatientCrudTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreatePatient_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var token = await TestAuthHelper.GetAuthTokenAsync(_client, _factory);
        TestAuthHelper.SetAuthToken(_client, token);

        // Complete onboarding first
        await CompleteOnboardingAsync();

        var request = new
        {
            fullName = "John Doe",
            gender = 1, // Male
            dateOfBirth = new DateTime(1990, 1, 1),
            cityGeoNameId = (int?)null,
            phoneNumbers = new[]
            {
                new { phoneNumber = "+1234567890", isPrimary = true }
            },
            chronicDiseaseIds = Array.Empty<Guid>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/patients", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<PatientResponse>();
        result.Should().NotBeNull();
        result!.FullName.Should().Be("John Doe");
        result.PatientCode.Should().StartWith("PAT-");
        result.Age.Should().BeGreaterThan(30);
    }

    [Fact]
    public async Task GetPatients_ShouldReturnPaginatedList()
    {
        // Arrange
        var token = await TestAuthHelper.GetAuthTokenAsync(_client, _factory);
        TestAuthHelper.SetAuthToken(_client, token);
        await CompleteOnboardingAsync();

        // Create a patient first
        await CreatePatientAsync("Jane Smith");

        // Act
        var response = await _client.GetAsync("/api/patients?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetPatientById_WithValidId_ShouldReturnPatient()
    {
        // Arrange
        var token = await TestAuthHelper.GetAuthTokenAsync(_client, _factory);
        TestAuthHelper.SetAuthToken(_client, token);
        await CompleteOnboardingAsync();

        var createdPatient = await CreatePatientAsync("Bob Johnson");

        // Act
        var response = await _client.GetAsync($"/api/patients/{createdPatient.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PatientResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(createdPatient.Id);
        result.FullName.Should().Be("Bob Johnson");
    }

    [Fact]
    public async Task UpdatePatient_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var token = await TestAuthHelper.GetAuthTokenAsync(_client, _factory);
        TestAuthHelper.SetAuthToken(_client, token);
        await CompleteOnboardingAsync();

        var createdPatient = await CreatePatientAsync("Alice Brown");

        var updateRequest = new
        {
            fullName = "Alice Brown Updated",
            gender = 2, // Female
            dateOfBirth = new DateTime(1985, 5, 15),
            cityGeoNameId = (int?)null
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/patients/{createdPatient.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PatientResponse>();
        result.Should().NotBeNull();
        result!.FullName.Should().Be("Alice Brown Updated");
    }

    [Fact]
    public async Task DeletePatient_WithValidId_ShouldReturnNoContent()
    {
        // Arrange
        var token = await TestAuthHelper.GetAuthTokenAsync(_client, _factory);
        TestAuthHelper.SetAuthToken(_client, token);
        await CompleteOnboardingAsync();

        var createdPatient = await CreatePatientAsync("Charlie Wilson");

        // Act
        var response = await _client.DeleteAsync($"/api/patients/{createdPatient.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify patient is deleted
        var getResponse = await _client.GetAsync($"/api/patients/{createdPatient.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreatePatient_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new
        {
            fullName = "Unauthorized User",
            gender = 1,
            dateOfBirth = new DateTime(1990, 1, 1),
            phoneNumbers = new[] { new { phoneNumber = "+1234567890", isPrimary = true } },
            chronicDiseaseIds = Array.Empty<Guid>()
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/patients", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task CompleteOnboardingAsync()
    {
        var onboardingRequest = new
        {
            clinicName = "Test Clinic",
            branchName = "Main Branch",
            address = "123 Test St",
            cityGeoNameId = 1,
            phoneNumbers = new[] { "+1234567890" },
            specializationId = Guid.Empty, // Will use first available
            subscriptionPlanId = Guid.Empty // Will use first available
        };

        await _client.PostAsJsonAsync("/api/onboarding/complete", onboardingRequest);
    }

    private async Task<PatientResponse> CreatePatientAsync(string fullName)
    {
        var request = new
        {
            fullName,
            gender = 1,
            dateOfBirth = new DateTime(1990, 1, 1),
            cityGeoNameId = (int?)null,
            phoneNumbers = new[] { new { phoneNumber = "+1234567890", isPrimary = true } },
            chronicDiseaseIds = Array.Empty<Guid>()
        };

        var response = await _client.PostAsJsonAsync("/api/patients", request);
        return (await response.Content.ReadFromJsonAsync<PatientResponse>())!;
    }

    private record PatientResponse(
        Guid Id,
        string PatientCode,
        string FullName,
        int Gender,
        DateTime DateOfBirth,
        int Age);

    private record PaginatedResponse(List<PatientResponse> Items, int TotalCount, int PageNumber, int PageSize);
}

