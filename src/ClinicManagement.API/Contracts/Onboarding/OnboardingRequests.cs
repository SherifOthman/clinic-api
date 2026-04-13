namespace ClinicManagement.API.Contracts.Onboarding;

public record CompleteOnboardingRequest(
    string ClinicName,
    Guid SubscriptionPlanId,
    string BranchName,
    string AddressLine,
    string? CityNameEn,
    string? CityNameAr,
    string? StateNameEn,
    string? StateNameAr,
    bool ProvideMedicalServices,
    Guid? SpecializationId,
    string? CountryCode);
