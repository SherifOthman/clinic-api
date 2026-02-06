using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Onboarding.Queries.GetSubscriptionPlans;

public record GetSubscriptionPlansQuery : IRequest<Result<IEnumerable<SubscriptionPlanDto>>>;
