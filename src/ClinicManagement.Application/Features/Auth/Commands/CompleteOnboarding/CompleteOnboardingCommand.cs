using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.CompleteOnboarding;

public record CompleteOnboardingCommand(
    string ClinicName,
    int SubscriptionPlanId,
    string BranchName,
    string BranchAddress,
    int CountryId,
    int? StateId,
    int CityId,
    List<BranchPhoneNumberDto> BranchPhoneNumbers
) : IRequest<Result<int>>;

public record BranchPhoneNumberDto(
    string PhoneNumber,
    string? Label = null
);
