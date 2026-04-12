using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ClinicManagement.Integration.Tests.Common;

public static class AuthHelper
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    // Registers a new user and returns the email used.
    public static async Task<string> RegisterAsync(HttpClient client, string? email = null, string gender = "Male")
    {
        email ??= $"user_{Guid.NewGuid():N}@test.com";
        var username = $"user_{Guid.NewGuid():N}";

        await client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "Test", lastName = "User",
            userName = username, email,
            password = "Test@1234!",
            phoneNumber = "+966500000001",
            gender,
        });

        return email;
    }

    // Logs in using mobile client type so the token is returned in the body (not a cookie).
    public static async Task<string?> LoginAsync(HttpClient client, string email, string password = "Test@1234!")
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { emailOrUsername = email, password }),
        };
        request.Headers.Add("X-Client-Type", "mobile");

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        return json.TryGetProperty("accessToken", out var token) ? token.GetString() : null;
    }

    public static void SetBearerToken(this HttpClient client, string token) =>
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
}
