namespace ClinicManagement.API.Contracts.Onboarding;

public record CompleteOnboardingRequest(
    string ClinicName,
    Guid SubscriptionPlanId,
    string BranchName,
    string AddressLine,
    int CountryGeoNameId,
    int StateGeoNameId,
    int CityGeoNameId
);
