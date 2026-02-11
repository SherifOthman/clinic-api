using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace ClinicManagement.IntegrationTests.Auth;

public class RegisterTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public RegisterTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var request = new
        {
            email = $"test{Guid.NewGuid()}@example.com",
            password = "Test123!@#",
            confirmPassword = "Test123!@#",
            firstName = "John",
            lastName = "Doe",
            userType = 1 // ClinicOwner
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        result.Should().NotBeNull();
        result!.Message.Should().Contain("registered");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"duplicate{Guid.NewGuid()}@example.com";
        var request = new
        {
            email,
            password = "Test123!@#",
            confirmPassword = "Test123!@#",
            firstName = "John",
            lastName = "Doe",
            userType = 1
        };

        // Register first time
        await _client.PostAsJsonAsync("/api/auth/register", request);

        // Act - Try to register again with same email
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            email = "invalid-email",
            password = "Test123!@#",
            confirmPassword = "Test123!@#",
            firstName = "John",
            lastName = "Doe",
            userType = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            email = $"test{Guid.NewGuid()}@example.com",
            password = "weak",
            confirmPassword = "weak",
            firstName = "John",
            lastName = "Doe",
            userType = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithMismatchedPasswords_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            email = $"test{Guid.NewGuid()}@example.com",
            password = "Test123!@#",
            confirmPassword = "Different123!@#",
            firstName = "John",
            lastName = "Doe",
            userType = 1
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private record RegisterResponse(string Message);
}
