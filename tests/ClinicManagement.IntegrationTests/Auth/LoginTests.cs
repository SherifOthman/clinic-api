using System.Net;
using System.Net.Http.Json;
using ClinicManagement.IntegrationTests.Helpers;
using FluentAssertions;

namespace ClinicManagement.IntegrationTests.Auth;

public class LoginTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public LoginTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnTokens()
    {
        // Arrange
        var email = $"test{Guid.NewGuid()}@example.com";
        var password = "Test123!@#";

        // Register user first
        await RegisterUserAsync(email, password);

        var loginRequest = new
        {
            emailOrUsername = email,
            password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new
        {
            emailOrUsername = "nonexistent@example.com",
            password = "Test123!@#"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"test{Guid.NewGuid()}@example.com";
        var password = "Test123!@#";

        // Register user first
        await RegisterUserAsync(email, password);

        var loginRequest = new
        {
            emailOrUsername = email,
            password = "WrongPassword123!@#"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ShouldSetRefreshTokenCookie()
    {
        // Arrange
        var email = $"test{Guid.NewGuid()}@example.com";
        var password = "Test123!@#";

        await RegisterUserAsync(email, password);

        var loginRequest = new
        {
            emailOrUsername = email,
            password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.Headers.TryGetValues("Set-Cookie", out var cookies);
        cookies.Should().NotBeNull();
        cookies!.Any(c => c.Contains("refreshToken")).Should().BeTrue();
    }

    private async Task RegisterUserAsync(string email, string password)
    {
        var userName = email.Split('@')[0];
        var registerRequest = new
        {
            email,
            password,
            firstName = "Test",
            lastName = "User",
            userName,
            phoneNumber = (string?)null
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        
        // Confirm email for test user
        await _factory.ConfirmEmailAsync(email);
    }

    private record LoginResponse(string AccessToken, string RefreshToken);
}
