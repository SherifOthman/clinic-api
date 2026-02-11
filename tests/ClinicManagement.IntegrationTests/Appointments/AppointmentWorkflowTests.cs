using System.Net;
using System.Net.Http.Json;
using ClinicManagement.IntegrationTests.Helpers;
using FluentAssertions;

namespace ClinicManagement.IntegrationTests.Appointments;

public class AppointmentWorkflowTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public AppointmentWorkflowTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateAppointment_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var token = await TestAuthHelper.GetAuthTokenAsync(_client, _factory);
        TestAuthHelper.SetAuthToken(_client, token);
        await CompleteOnboardingAsync();

        var patient = await CreatePatientAsync();
        var appointmentTypeId = await GetFirstAppointmentTypeIdAsync();

        var request = new
        {
            patientId = patient.Id,
            doctorId = Guid.Empty, // Will be set from onboarding
            clinicBranchId = Guid.Empty, // Will be set from onboarding
            scheduledAt = DateTime.UtcNow.AddDays(1),
            appointmentTypeId
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/appointments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<AppointmentResponse>();
        result.Should().NotBeNull();
        result!.AppointmentNumber.Should().StartWith("APT-");
        result.Status.Should().Be(1); // Pending
        result.QueueNumber.Should().BeGreaterThan((short)0);
    }

    [Fact]
    public async Task ConfirmAppointment_WhenPending_ShouldChangeStatusToConfirmed()
    {
        // Arrange
        var token = await TestAuthHelper.GetAuthTokenAsync(_client, _factory);
        TestAuthHelper.SetAuthToken(_client, token);
        await CompleteOnboardingAsync();

        var appointment = await CreateAppointmentAsync();

        // Act
        var response = await _client.PostAsync($"/api/appointments/{appointment.Id}/confirm", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AppointmentResponse>();
        result!.Status.Should().Be(2); // Confirmed
    }

    [Fact]
    public async Task CompleteAppointment_WhenConfirmed_ShouldChangeStatusToCompleted()
    {
        // Arrange
        var token = await TestAuthHelper.GetAuthTokenAsync(_client, _factory);
        TestAuthHelper.SetAuthToken(_client, token);
        await CompleteOnboardingAsync();

        var appointment = await CreateAppointmentAsync();
        await _client.PostAsync($"/api/appointments/{appointment.Id}/confirm", null);

        // Act
        var response = await _client.PostAsync($"/api/appointments/{appointment.Id}/complete", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AppointmentResponse>();
        result!.Status.Should().Be(3); // Completed
    }

    [Fact]
    public async Task CancelAppointment_WhenPending_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var token = await TestAuthHelper.GetAuthTokenAsync(_client, _factory);
        TestAuthHelper.SetAuthToken(_client, token);
        await CompleteOnboardingAsync();

        var appointment = await CreateAppointmentAsync();

        // Act
        var response = await _client.PostAsync($"/api/appointments/{appointment.Id}/cancel", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AppointmentResponse>();
        result!.Status.Should().Be(4); // Cancelled
    }

    [Fact]
    public async Task GetAppointments_ShouldReturnPaginatedList()
    {
        // Arrange
        var token = await TestAuthHelper.GetAuthTokenAsync(_client, _factory);
        TestAuthHelper.SetAuthToken(_client, token);
        await CompleteOnboardingAsync();

        await CreateAppointmentAsync();

        // Act
        var response = await _client.GetAsync("/api/appointments?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse>();
        result.Should().NotBeNull();
        result!.Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAppointmentById_WithValidId_ShouldReturnAppointment()
    {
        // Arrange
        var token = await TestAuthHelper.GetAuthTokenAsync(_client, _factory);
        TestAuthHelper.SetAuthToken(_client, token);
        await CompleteOnboardingAsync();

        var appointment = await CreateAppointmentAsync();

        // Act
        var response = await _client.GetAsync($"/api/appointments/{appointment.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AppointmentResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(appointment.Id);
    }

    private async Task CompleteOnboardingAsync()
    {
        var onboardingRequest = new
        {
            clinicName = "Test Clinic",
            subscriptionPlanId = TestConstants.BasicPlanId,
            branchName = "Main Branch",
            addressLine = "123 Test St",
            countryGeoNameId = 1,
            stateGeoNameId = 1,
            cityGeoNameId = 1
        };

        await _client.PostAsJsonAsync("/api/onboarding/complete", onboardingRequest);
    }

    private async Task<PatientResponse> CreatePatientAsync()
    {
        var request = new
        {
            fullName = "Test Patient",
            gender = 1,
            dateOfBirth = new DateTime(1990, 1, 1),
            cityGeoNameId = (int?)null,
            phoneNumbers = new[] { new { phoneNumber = "+1234567890", isPrimary = true } },
            chronicDiseaseIds = Array.Empty<Guid>()
        };

        var response = await _client.PostAsJsonAsync("/api/patients", request);
        return (await response.Content.ReadFromJsonAsync<PatientResponse>())!;
    }

    private async Task<AppointmentResponse> CreateAppointmentAsync()
    {
        var patient = await CreatePatientAsync();
        var appointmentTypeId = await GetFirstAppointmentTypeIdAsync();

        var request = new
        {
            patientId = patient.Id,
            doctorId = Guid.Empty,
            clinicBranchId = Guid.Empty,
            scheduledAt = DateTime.UtcNow.AddDays(1),
            appointmentTypeId
        };

        var response = await _client.PostAsJsonAsync("/api/appointments", request);
        return (await response.Content.ReadFromJsonAsync<AppointmentResponse>())!;
    }

    private async Task<Guid> GetFirstAppointmentTypeIdAsync()
    {
        // This would need to be seeded or created in the test database
        // For now, return a placeholder
        return Guid.NewGuid();
    }

    private record PatientResponse(Guid Id, string PatientCode, string FullName);
    private record AppointmentResponse(
        Guid Id,
        string AppointmentNumber,
        int Status,
        short QueueNumber,
        DateTime AppointmentDate);
    private record PaginatedResponse(List<AppointmentResponse> Items, int TotalCount);
}

