using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Onboarding.Commands;

public record CompleteOnboarding(
    string ClinicName,
    Guid SubscriptionPlanId,
    string BranchName,
    string AddressLine,
    int? StateGeonameId,
    int? CityGeonameId,
    bool ProvideMedicalServices,
    Guid? SpecializationId,
    string? CountryCode
) : IRequest<Result>;
