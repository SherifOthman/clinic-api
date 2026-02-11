using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ClinicManagement.IntegrationTests.Helpers;

/// <summary>
/// Helper class for authentication in integration tests
/// </summary>
public static class TestAuthHelper
{
    public static async Task<string> RegisterAndLoginAsync(
        HttpClient client,
        string email,
        string password,
        string firstName = "Test",
        string lastName = "User")
    {
        // Generate username from email (take part before @)
        var userName = email.Split('@')[0];
        
        // Register
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
        
        if (!registerResponse.IsSuccessStatusCode)
        {
            var error = await registerResponse.Content.ReadAsStringAsync();
            throw new Exception($"Registration failed: {error}");
        }

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
        string email = "test@example.com",
        string password = "Test123!@#")
    {
        return await RegisterAndLoginAsync(client, email, password);
    }

    private record LoginResponse(string AccessToken, string RefreshToken);
}
