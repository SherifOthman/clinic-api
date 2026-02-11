using System.Net;
using System.Net.Http.Json;
using ClinicManagement.IntegrationTests.Helpers;
using FluentAssertions;

namespace ClinicManagement.IntegrationTests.Auth;

public class LoginTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public LoginTests(TestWebApplicationFactory factory)
    {
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
            email,
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
    public async Task Login_WithInvalidEmail_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new
        {
            email = "nonexistent@example.com",
            password = "Test123!@#"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var email = $"test{Guid.NewGuid()}@example.com";
        var password = "Test123!@#";

        // Register user first
        await RegisterUserAsync(email, password);

        var loginRequest = new
        {
            email,
            password = "WrongPassword123!@#"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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
            email,
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
        var registerRequest = new
        {
            email,
            password,
            confirmPassword = password,
            firstName = "Test",
            lastName = "User",
            userType = 1
        };

        await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
    }

    private record LoginResponse(string AccessToken, string RefreshToken);
}
