using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClinicManagement.Integration.Tests.Common;
using FluentAssertions;

namespace ClinicManagement.Integration.Tests.Auth;

public class AuthEndpointsTests : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public AuthEndpointsTests(IntegrationTestFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── Register ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ShouldReturn201_WithValidData()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Test",
            lastName = "User",
            userName = $"testuser_{Guid.NewGuid():N}",
            email = $"test_{Guid.NewGuid():N}@test.com",
            password = "Test@1234!",
            phoneNumber = "+966500000001",
            gender = "Male"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_ShouldReturn400_WithMissingFields()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "incomplete@test.com"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ShouldReturn400_WhenEmailAlreadyExists()
    {
        var email = $"dup_{Guid.NewGuid():N}@test.com";

        await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "First", lastName = "User",
            userName = $"user_{Guid.NewGuid():N}",
            email, password = "Test@1234!", phoneNumber = "+966500000002", gender = "Male"
        });

        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Second", lastName = "User",
            userName = $"user_{Guid.NewGuid():N}",
            email, password = "Test@1234!", phoneNumber = "+966500000003", gender = "Female"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ShouldReturn400_WithWrongCredentials()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new
            {
                emailOrUsername = "nobody@test.com",
                password = "WrongPassword!"
            })
        };
        request.Headers.Add("X-Client-Type", "mobile");

        var response = await _client.SendAsync(request);
        // Login failures return 400 (HandleResult maps all failures to BadRequest)
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldReturnToken_AfterSuccessfulRegistration()
    {
        var email = await AuthHelper.RegisterAsync(_client);

        var token = await AuthHelper.LoginAsync(_client, email);

        token.Should().NotBeNullOrEmpty();
    }

    // ── GetMe ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetMe_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _client.GetAsync("/api/auth/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMe_ShouldReturnUserData_WhenAuthenticated()
    {
        var email = await AuthHelper.RegisterAsync(_client, gender: "Female");
        var token = await AuthHelper.LoginAsync(_client, email);
        _client.SetBearerToken(token!);

        var response = await _client.GetAsync("/api/auth/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        body.GetProperty("email").GetString().Should().Be(email);
        body.GetProperty("gender").GetString().Should().Be("Female");
    }

    // ── UpdateProfile ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateProfile_ShouldReturn204_WhenAuthenticated()
    {
        var email = await AuthHelper.RegisterAsync(_client);
        var token = await AuthHelper.LoginAsync(_client, email);
        _client.SetBearerToken(token!);

        var response = await _client.PutAsJsonAsync("/api/auth/profile", new
        {
            firstName = "Updated",
            lastName = "Name",
            userName = $"u{Guid.NewGuid():N}"[..15], // max 20 chars
            phoneNumber = "+966500000099",
            gender = "Male"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateProfile_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _client.PutAsJsonAsync("/api/auth/profile", new
        {
            firstName = "Test", lastName = "User",
            userName = "testuser", phoneNumber = "+966500000001", gender = "Male"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
