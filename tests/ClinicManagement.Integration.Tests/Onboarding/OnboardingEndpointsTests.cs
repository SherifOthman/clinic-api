using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClinicManagement.Integration.Tests.Common;
using FluentAssertions;

namespace ClinicManagement.Integration.Tests.Onboarding;

public class OnboardingEndpointsTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public OnboardingEndpointsTests(IntegrationTestFactory factory)
    {
        _factory = factory;
    }

    private async Task<(HttpClient client, string token)> CreateFreshOwnerClientAsync()
    {
        var client = _factory.CreateClient();
        var email  = await AuthHelper.RegisterAsync(client);
        var token  = await AuthHelper.LoginAsync(client, email);
        client.SetBearerToken(token!);
        return (client, token!);
    }

    private async Task<string> GetFirstPlanIdAsync()
    {
        var client   = _factory.CreateClient();
        var response = await client.GetAsync("/api/subscription-plans");
        var body     = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        return body.EnumerateArray().First().GetProperty("id").GetString()!;
    }

    // ── Auth guards ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CompleteOnboarding_ShouldReturn401_WhenNotAuthenticated()
    {
        var response = await _factory.CreateClient().PostAsJsonAsync("/api/onboarding/complete", new
        {
            clinicName = "Test", subscriptionPlanId = Guid.NewGuid(),
            branchName = "Main", addressLine = "123 St", provideMedicalServices = "no"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CompleteOnboarding_ShouldReturn200_WithValidData()
    {
        var (client, _) = await CreateFreshOwnerClientAsync();
        var planId      = await GetFirstPlanIdAsync();

        var response = await client.PostAsJsonAsync("/api/onboarding/complete", new
        {
            clinicName           = "My Test Clinic",
            subscriptionPlanId   = planId,
            branchName           = "Main Branch",
            addressLine          = "123 Medical Street",
            stateGeonameId       = (int?)null,
            cityGeonameId        = (int?)null,
            countryCode          = (string?)null,
            provideMedicalServices = "no"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CompleteOnboarding_ShouldSetOnboardingCompleted_OnGetMe()
    {
        var (client, _) = await CreateFreshOwnerClientAsync();
        var planId      = await GetFirstPlanIdAsync();

        await client.PostAsJsonAsync("/api/onboarding/complete", new
        {
            clinicName           = "Onboarded Clinic",
            subscriptionPlanId   = planId,
            branchName           = "Main Branch",
            addressLine          = "456 Street",
            provideMedicalServices = "no"
        });

        var meResponse = await client.GetAsync("/api/auth/me");
        var me         = await meResponse.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        me.GetProperty("onboardingCompleted").GetBoolean().Should().BeTrue();
    }

    // ── Validation ────────────────────────────────────────────────────────────

    [Fact]
    public async Task CompleteOnboarding_ShouldReturn400_WithMissingClinicName()
    {
        var (client, _) = await CreateFreshOwnerClientAsync();
        var planId      = await GetFirstPlanIdAsync();

        var response = await client.PostAsJsonAsync("/api/onboarding/complete", new
        {
            clinicName           = "",
            subscriptionPlanId   = planId,
            branchName           = "Main Branch",
            addressLine          = "123 Street",
            provideMedicalServices = "no"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CompleteOnboarding_ShouldReturn400_WhenDoctorWithoutSpecialization()
    {
        var (client, _) = await CreateFreshOwnerClientAsync();
        var planId      = await GetFirstPlanIdAsync();

        var response = await client.PostAsJsonAsync("/api/onboarding/complete", new
        {
            clinicName             = "Clinic",
            subscriptionPlanId     = planId,
            branchName             = "Main",
            addressLine            = "123 St",
            provideMedicalServices = "yes",
            specializationId       = (Guid?)null  // required when yes
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
