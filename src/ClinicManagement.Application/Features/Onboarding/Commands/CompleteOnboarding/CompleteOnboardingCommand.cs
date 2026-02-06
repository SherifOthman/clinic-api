using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Onboarding.Commands.CompleteOnboarding;

public record CompleteOnboardingCommand(CompleteOnboardingDto Dto) : IRequest<Result>;
