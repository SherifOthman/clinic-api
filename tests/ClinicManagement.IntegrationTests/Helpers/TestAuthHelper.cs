using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ClinicManagement.IntegrationTests.Helpers;

/// <summary>
/// Helper class for authentication in integration tests
/// </summary>
public static class TestAuthHelper
{
    public static async Task<string> RegisterAndLoginAsync(
        TestWebApplicationFactory factory,
        HttpClient client,
        string email,
        string password,
        string firstName = "Test",
        string lastName = "User")
    {
        // Generate username from email (take part before @)
        var userName = email.Split('@')[0];
        
        // Try to register (ignore if already exists)
        var registerRequest = new
        {
            email,
            password,
            firstName,
            lastName,
            userName,
            phoneNumber = (string?)null
        };

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", registerRequest);
        
        // If registration fails with duplicate email, that's okay - we'll just login
        if (!registerResponse.IsSuccessStatusCode)
        {
            var error = await registerResponse.Content.ReadAsStringAsync();
            // Only throw if it's not a duplicate email error
            if (!error.Contains("Email is already registered", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"Registration failed: {error}");
            }
        }
        
        // Confirm email for test user
        await factory.ConfirmEmailAsync(email);

        // Login
        var loginRequest = new
        {
            email,
            password
        };

        var loginResponse = await client.PostAsJsonAsync("/api/auth/login", loginRequest);
        
        if (!loginResponse.IsSuccessStatusCode)
        {
            var error = await loginResponse.Content.ReadAsStringAsync();
            throw new Exception($"Login failed: {error}");
        }

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        return loginResult?.AccessToken ?? throw new Exception("No access token received");
    }

    public static void SetAuthToken(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task<string> GetAuthTokenAsync(
        HttpClient client,
        TestWebApplicationFactory factory,
        string email = "test@example.com",
        string password = "Test123!@#")
    {
        return await RegisterAndLoginAsync(factory, client, email, password);
    }

    private record LoginResponse(string AccessToken, string RefreshToken);
}
