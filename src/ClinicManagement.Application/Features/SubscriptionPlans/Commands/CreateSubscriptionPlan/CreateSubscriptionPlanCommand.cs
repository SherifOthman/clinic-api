using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.SubscriptionPlans.Commands.CreateSubscriptionPlan;

public record CreateSubscriptionPlanCommand : IRequest<Result<SubscriptionPlanDto>>
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int DurationDays { get; init; }
    public int MaxUsers { get; init; }
    public int MaxPatients { get; init; }
    public bool IsActive { get; init; } = true;
}
