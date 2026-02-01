using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.CompleteOnboarding;

public record CompleteOnboardingCommand(
    string ClinicName,
    Guid SubscriptionPlanId,
    string BranchName,
    string BranchAddress,
    int CountryId,
    int? StateId,
    int CityId,
    List<BranchPhoneNumberDto> BranchPhoneNumbers
) : IRequest<Result<Guid>>;

public record BranchPhoneNumberDto(
    string PhoneNumber,
    string? Label = null
);
