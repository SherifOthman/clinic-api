using ClinicManagement.Application.DTOs;
using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.SubscriptionPlans.Queries.GetSubscriptionPlan;

public record GetSubscriptionPlanQuery(Guid Id) : IRequest<Result<SubscriptionPlanDto>>;
