using System.Net;
using System.Net.Http.Json;
using ClinicManagement.IntegrationTests.Helpers;
using FluentAssertions;

namespace ClinicManagement.IntegrationTests.Invoices;

public class InvoicePaymentFlowTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public InvoicePaymentFlowTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateInvoice_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var token = await TestAuthHelper.GetAuthTokenAsync(_client, _factory);
        TestAuthHelper.SetAuthToken(_client, token);
        await CompleteOnboardingAsync();

        var patient = await CreatePatientAsync();

        var request = new
        {
            patientId = patient.Id,
            items = new[]
            {
                new
                {
                    medicalServiceId = Guid.NewGuid(),
                    quantity = 1,
                    unitPrice = 100.00m
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/invoices", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<InvoiceResponse>();
        result.Should().NotBeNull();
        result!.InvoiceNumber.Should().StartWith("INV-");
        result.Status.Should().Be(1); // Draft
        result.TotalAmount.Should().Be(100.00m);
    }

    [Fact]
    public async Task RecordPayment_ForIssuedInvoice_ShouldUpdateStatus()
    {
        // Arrange
        var token = await TestAuthHelper.GetAuthTokenAsync(_client, _factory);
        TestAuthHelper.SetAuthToken(_client, token);
        await CompleteOnboardingAsync();

        var invoice = await CreateAndIssueInvoiceAsync();

        var paymentRequest = new
        {
            amount = 50.00m,
            paymentMethod = 1, // Cash
            referenceNumber = "REF-123"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/invoices/{invoice.Id}/payments", paymentRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Verify invoice status changed to PartiallyPaid
        var invoiceResponse = await _client.GetAsync($"/api/invoices/{invoice.Id}");
        var updatedInvoice = await invoiceResponse.Content.ReadFromJsonAsync<InvoiceResponse>();
        updatedInvoice!.Status.Should().Be(3); // PartiallyPaid
        updatedInvoice.TotalPaid.Should().Be(50.00m);
        updatedInvoice.RemainingAmount.Should().Be(50.00m);
    }

    [Fact]
    public async Task RecordFullPayment_ShouldMarkInvoiceAsFullyPaid()
    {
        // Arrange
        var token = await TestAuthHelper.GetAuthTokenAsync(_client, _factory);
        TestAuthHelper.SetAuthToken(_client, token);
        await CompleteOnboardingAsync();

        var invoice = await CreateAndIssueInvoiceAsync();

        var paymentRequest = new
        {
            amount = 100.00m,
            paymentMethod = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/invoices/{invoice.Id}/payments", paymentRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        // Verify invoice is fully paid
        var invoiceResponse = await _client.GetAsync($"/api/invoices/{invoice.Id}");
        var updatedInvoice = await invoiceResponse.Content.ReadFromJsonAsync<InvoiceResponse>();
        updatedInvoice!.Status.Should().Be(4); // FullyPaid
        updatedInvoice.RemainingAmount.Should().Be(0m);
    }

    [Fact]
    public async Task RecordPayment_ExceedingInvoiceAmount_ShouldReturnBadRequest()
    {
        // Arrange
        var token = await TestAuthHelper.GetAuthTokenAsync(_client, _factory);
        TestAuthHelper.SetAuthToken(_client, token);
        await CompleteOnboardingAsync();

        var invoice = await CreateAndIssueInvoiceAsync();

        var paymentRequest = new
        {
            amount = 150.00m, // More than invoice amount
            paymentMethod = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/invoices/{invoice.Id}/payments", paymentRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CancelInvoice_WhenNotFullyPaid_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var token = await TestAuthHelper.GetAuthTokenAsync(_client, _factory);
        TestAuthHelper.SetAuthToken(_client, token);
        await CompleteOnboardingAsync();

        var invoice = await CreateAndIssueInvoiceAsync();

        var cancelRequest = new
        {
            reason = "Customer request"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/invoices/{invoice.Id}/cancel", cancelRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<InvoiceResponse>();
        result!.Status.Should().Be(5); // Cancelled
    }

    [Fact]
    public async Task GetInvoicePayments_ShouldReturnAllPayments()
    {
        // Arrange
        var token = await TestAuthHelper.GetAuthTokenAsync(_client, _factory);
        TestAuthHelper.SetAuthToken(_client, token);
        await CompleteOnboardingAsync();

        var invoice = await CreateAndIssueInvoiceAsync();

        // Record multiple payments
        await RecordPaymentAsync(invoice.Id, 30m);
        await RecordPaymentAsync(invoice.Id, 20m);

        // Act
        var response = await _client.GetAsync($"/api/invoices/{invoice.Id}/payments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payments = await response.Content.ReadFromJsonAsync<List<PaymentResponse>>();
        payments.Should().NotBeNull();
        payments!.Should().HaveCount(2);
        payments.Sum(p => p.Amount).Should().Be(50m);
    }

    [Fact]
    public async Task GetPatientInvoices_ShouldReturnAllInvoicesForPatient()
    {
        // Arrange
        var token = await TestAuthHelper.GetAuthTokenAsync(_client, _factory);
        TestAuthHelper.SetAuthToken(_client, token);
        await CompleteOnboardingAsync();

        var patient = await CreatePatientAsync();
        await CreateInvoiceForPatientAsync(patient.Id);
        await CreateInvoiceForPatientAsync(patient.Id);

        // Act
        var response = await _client.GetAsync($"/api/patients/{patient.Id}/invoices");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var invoices = await response.Content.ReadFromJsonAsync<List<InvoiceResponse>>();
        invoices.Should().NotBeNull();
        invoices!.Should().HaveCountGreaterThanOrEqualTo(2);
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

    private async Task<InvoiceResponse> CreateInvoiceForPatientAsync(Guid patientId)
    {
        var request = new
        {
            patientId,
            items = new[]
            {
                new { medicalServiceId = Guid.NewGuid(), quantity = 1, unitPrice = 100.00m }
            }
        };

        var response = await _client.PostAsJsonAsync("/api/invoices", request);
        return (await response.Content.ReadFromJsonAsync<InvoiceResponse>())!;
    }

    private async Task<InvoiceResponse> CreateAndIssueInvoiceAsync()
    {
        var patient = await CreatePatientAsync();
        return await CreateInvoiceForPatientAsync(patient.Id);
    }

    private async Task RecordPaymentAsync(Guid invoiceId, decimal amount)
    {
        var paymentRequest = new
        {
            amount,
            paymentMethod = 1
        };

        await _client.PostAsJsonAsync($"/api/invoices/{invoiceId}/payments", paymentRequest);
    }

    private record PatientResponse(Guid Id, string PatientCode);
    private record InvoiceResponse(
        Guid Id,
        string InvoiceNumber,
        int Status,
        decimal TotalAmount,
        decimal TotalPaid,
        decimal RemainingAmount);
    private record PaymentResponse(Guid Id, decimal Amount, int PaymentMethod);
}

