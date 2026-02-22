using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Onboarding.Commands;

public record CompleteOnboarding(
    string ClinicName,
    int SubscriptionPlanId,
    string BranchName,
    string AddressLine,
    int CountryGeoNameId,
    int StateGeoNameId,
    int CityGeoNameId
) : IRequest<Result>;
