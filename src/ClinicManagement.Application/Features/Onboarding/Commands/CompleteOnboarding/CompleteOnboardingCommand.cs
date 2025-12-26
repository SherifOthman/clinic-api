using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Onboarding.Commands.CompleteOnboarding;

public class CompleteOnboardingCommand : IRequest<Result<ClinicDto>>
{
    public string ClinicName { get; set; } = string.Empty;
    public string? ClinicPhone { get; set; }
    public int SubscriptionPlanId { get; set; }
}