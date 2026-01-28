using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.SubscriptionPlans.Commands.DeleteSubscriptionPlan;

public record DeleteSubscriptionPlanCommand(int Id) : IRequest<Result>;
