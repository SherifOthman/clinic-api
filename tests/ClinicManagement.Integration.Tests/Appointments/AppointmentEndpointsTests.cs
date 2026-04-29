using System.Net;
using System.Net.Http.Json;
using ClinicManagement.Integration.Tests.Common;
using FluentAssertions;

namespace ClinicManagement.Integration.Tests.Appointments;

public class AppointmentEndpointsTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;

    public AppointmentEndpointsTests(IntegrationTestFactory factory)
        => _factory = factory;

    private async Task<HttpClient> CreateOwnerClientAsync()
    {
        var client = _factory.CreateClient();
        var email  = await AuthHelper.RegisterAsync(client);
        var token  = await AuthHelper.LoginAsync(client, email);
        client.SetBearerToken(token!);
        return client;
    }

    // ── Auth guards ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAppointments_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _factory.CreateClient()
            .GetAsync($"/api/appointments?date={DateOnly.FromDateTime(DateTime.Today):yyyy-MM-dd}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetDoctors_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _factory.CreateClient()
            .GetAsync($"/api/appointments/doctors?branchId={Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAppointment_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _factory.CreateClient()
            .PostAsJsonAsync("/api/appointments", new { });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Authenticated ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAppointments_ShouldReturn200_WhenAuthenticated()
    {
        var client   = await CreateOwnerClientAsync();
        var response = await client.GetAsync(
            $"/api/appointments?date={DateOnly.FromDateTime(DateTime.Today):yyyy-MM-dd}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateAppointment_ShouldReturn400_WhenInvalidDate()
    {
        var client   = await CreateOwnerClientAsync();
        var response = await client.PostAsJsonAsync("/api/appointments", new
        {
            branchId     = Guid.NewGuid(),
            patientId    = Guid.NewGuid(),
            doctorInfoId = Guid.NewGuid(),
            visitTypeId  = Guid.NewGuid(),
            date         = "not-a-date",
            type         = "Queue",
        });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAppointment_ShouldReturn400_WhenInvalidType()
    {
        var client   = await CreateOwnerClientAsync();
        var response = await client.PostAsJsonAsync("/api/appointments", new
        {
            branchId     = Guid.NewGuid(),
            patientId    = Guid.NewGuid(),
            doctorInfoId = Guid.NewGuid(),
            visitTypeId  = Guid.NewGuid(),
            date         = DateTime.Today.ToString("yyyy-MM-dd"),
            type         = "InvalidType",
        });
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateStatus_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _factory.CreateClient()
            .PatchAsJsonAsync($"/api/appointments/{Guid.NewGuid()}/status", new { status = "InProgress" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SetAppointmentType_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _factory.CreateClient()
            .PatchAsJsonAsync($"/api/appointments/doctors/{Guid.NewGuid()}/appointment-type",
                new { appointmentType = "Queue" });
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
