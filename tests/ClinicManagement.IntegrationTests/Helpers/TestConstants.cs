namespace ClinicManagement.IntegrationTests.Helpers;

/// <summary>
/// Constants for integration tests
/// </summary>
public static class TestConstants
{
    // Seeded subscription plan IDs
    public static readonly Guid BasicPlanId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid ProPlanId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    
    // Seeded specialization IDs
    public static readonly Guid GeneralPracticeId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid PediatricsId = Guid.Parse("44444444-4444-4444-4444-444444444444");
}
