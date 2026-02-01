using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.SubscriptionPlans.Commands.UpdateSubscriptionPlan;

public record UpdateSubscriptionPlanCommand : IRequest<Result<SubscriptionPlanDto>>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public int DurationDays { get; init; }
    public int MaxUsers { get; init; }
    public int MaxPatients { get; init; }
    public bool IsActive { get; init; }
}
