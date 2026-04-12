using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries;

public record CheckEmailAvailabilityQuery(string Email) : IRequest<AvailabilityDto>;
