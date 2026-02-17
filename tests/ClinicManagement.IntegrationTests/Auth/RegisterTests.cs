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
        var email = $"test{Guid.NewGuid()}@example.com";
        var request = new
        {
            email,
            password = "Test123!@#",
            firstName = "John",
            lastName = "Doe",
            userName = email.Split('@')[0],
            phoneNumber = (string?)null
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        result.Should().NotBeNull();
        result!.Message.Should().Contain("successful");
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
            firstName = "John",
            lastName = "Doe",
            userName = email.Split('@')[0], phoneNumber = (string?)null
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
        var email = "invalid-email";
        var request = new
        {
            email,
            password = "Test123!@#",
            firstName = "John",
            lastName = "Doe",
            userName = "invaliduser",
            phoneNumber = (string?)null
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
        var email = $"test{Guid.NewGuid()}@example.com";
        var request = new
        {
            email,
            password = "weak",
            firstName = "John",
            lastName = "Doe",
            userName = email.Split('@')[0],
            phoneNumber = (string?)null
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private record RegisterResponse(string Message);
}

