namespace ClinicManagement.API.Contracts.Onboarding;

public record CompleteOnboardingRequest(
    string ClinicName,
    Guid SubscriptionPlanId,
    string BranchName,
    string AddressLine,
    int? StateGeonameId,
    int? CityGeonameId,
    bool ProvideMedicalServices,
    Guid? SpecializationId,
    string? CountryCode);
