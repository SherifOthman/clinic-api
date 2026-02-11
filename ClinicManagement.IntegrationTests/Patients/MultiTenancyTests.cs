using System.Net;
using System.Net.Http.Json;
using ClinicManagement.IntegrationTests.Helpers;
using FluentAssertions;

namespace ClinicManagement.IntegrationTests.Patients;

public class MultiTenancyTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public MultiTenancyTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetPatients_ShouldOnlyReturnPatientsFromOwnClinic()
    {
        // Arrange - Create two separate clinics with their own patients
        var client1 = _factory.CreateClient();
        var client2 = _factory.CreateClient();

        var token1 = await TestAuthHelper.RegisterAndLoginAsync(
            client1,
            $"clinic1{Guid.NewGuid()}@example.com",
            "Test123!@#",
            "Clinic1",
            "Owner");

        var token2 = await TestAuthHelper.RegisterAndLoginAsync(
            client2,
            $"clinic2{Guid.NewGuid()}@example.com",
            "Test123!@#",
            "Clinic2",
            "Owner");

        TestAuthHelper.SetAuthToken(client1, token1);
        TestAuthHelper.SetAuthToken(client2, token2);

        // Complete onboarding for both
        await CompleteOnboardingAsync(client1);
        await CompleteOnboardingAsync(client2);

        // Create patients for each clinic
        var patient1 = await CreatePatientAsync(client1, "Clinic1 Patient");
        var patient2 = await CreatePatientAsync(client2, "Clinic2 Patient");

        // Act - Get patients for clinic 1
        var response1 = await client1.GetAsync("/api/patients?pageNumber=1&pageSize=100");
        var result1 = await response1.Content.ReadFromJsonAsync<PaginatedResponse>();

        // Act - Get patients for clinic 2
        var response2 = await client2.GetAsync("/api/patients?pageNumber=1&pageSize=100");
        var result2 = await response2.Content.ReadFromJsonAsync<PaginatedResponse>();

        // Assert - Each clinic should only see their own patients
        result1!.Items.Should().Contain(p => p.Id == patient1.Id);
        result1.Items.Should().NotContain(p => p.Id == patient2.Id);

        result2!.Items.Should().Contain(p => p.Id == patient2.Id);
        result2.Items.Should().NotContain(p => p.Id == patient1.Id);
    }

    [Fact]
    public async Task GetPatientById_FromDifferentClinic_ShouldReturnNotFound()
    {
        // Arrange - Create two clinics
        var client1 = _factory.CreateClient();
        var client2 = _factory.CreateClient();

        var token1 = await TestAuthHelper.RegisterAndLoginAsync(
            client1,
            $"clinic1{Guid.NewGuid()}@example.com",
            "Test123!@#");

        var token2 = await TestAuthHelper.RegisterAndLoginAsync(
            client2,
            $"clinic2{Guid.NewGuid()}@example.com",
            "Test123!@#");

        TestAuthHelper.SetAuthToken(client1, token1);
        TestAuthHelper.SetAuthToken(client2, token2);

        await CompleteOnboardingAsync(client1);
        await CompleteOnboardingAsync(client2);

        // Create patient in clinic 1
        var patient1 = await CreatePatientAsync(client1, "Clinic1 Patient");

        // Act - Try to access clinic 1's patient from clinic 2
        var response = await client2.GetAsync($"/api/patients/{patient1.Id}");

        // Assert - Should not be able to access
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePatient_FromDifferentClinic_ShouldReturnNotFound()
    {
        // Arrange
        var client1 = _factory.CreateClient();
        var client2 = _factory.CreateClient();

        var token1 = await TestAuthHelper.RegisterAndLoginAsync(
            client1,
            $"clinic1{Guid.NewGuid()}@example.com",
            "Test123!@#");

        var token2 = await TestAuthHelper.RegisterAndLoginAsync(
            client2,
            $"clinic2{Guid.NewGuid()}@example.com",
            "Test123!@#");

        TestAuthHelper.SetAuthToken(client1, token1);
        TestAuthHelper.SetAuthToken(client2, token2);

        await CompleteOnboardingAsync(client1);
        await CompleteOnboardingAsync(client2);

        var patient1 = await CreatePatientAsync(client1, "Clinic1 Patient");

        var updateRequest = new
        {
            fullName = "Hacked Name",
            gender = 1,
            dateOfBirth = new DateTime(1990, 1, 1),
            cityGeoNameId = (int?)null
        };

        // Act - Try to update clinic 1's patient from clinic 2
        var response = await client2.PutAsJsonAsync($"/api/patients/{patient1.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify patient was not updated
        var getResponse = await client1.GetAsync($"/api/patients/{patient1.Id}");
        var patient = await getResponse.Content.ReadFromJsonAsync<PatientResponse>();
        patient!.FullName.Should().Be("Clinic1 Patient");
    }

    [Fact]
    public async Task DeletePatient_FromDifferentClinic_ShouldReturnNotFound()
    {
        // Arrange
        var client1 = _factory.CreateClient();
        var client2 = _factory.CreateClient();

        var token1 = await TestAuthHelper.RegisterAndLoginAsync(
            client1,
            $"clinic1{Guid.NewGuid()}@example.com",
            "Test123!@#");

        var token2 = await TestAuthHelper.RegisterAndLoginAsync(
            client2,
            $"clinic2{Guid.NewGuid()}@example.com",
            "Test123!@#");

        TestAuthHelper.SetAuthToken(client1, token1);
        TestAuthHelper.SetAuthToken(client2, token2);

        await CompleteOnboardingAsync(client1);
        await CompleteOnboardingAsync(client2);

        var patient1 = await CreatePatientAsync(client1, "Clinic1 Patient");

        // Act - Try to delete clinic 1's patient from clinic 2
        var response = await client2.DeleteAsync($"/api/patients/{patient1.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify patient still exists
        var getResponse = await client1.GetAsync($"/api/patients/{patient1.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task CompleteOnboardingAsync(HttpClient client)
    {
        var onboardingRequest = new
        {
            clinicName = $"Test Clinic {Guid.NewGuid()}",
            branchName = "Main Branch",
            address = "123 Test St",
            cityGeoNameId = 1,
            phoneNumbers = new[] { "+1234567890" },
            specializationId = Guid.Empty,
            subscriptionPlanId = Guid.Empty
        };

        await client.PostAsJsonAsync("/api/onboarding/complete", onboardingRequest);
    }

    private async Task<PatientResponse> CreatePatientAsync(HttpClient client, string fullName)
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

        var response = await client.PostAsJsonAsync("/api/patients", request);
        return (await response.Content.ReadFromJsonAsync<PatientResponse>())!;
    }

    private record PatientResponse(Guid Id, string PatientCode, string FullName);
    private record PaginatedResponse(List<PatientResponse> Items, int TotalCount);
}
