using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries;

public record CheckEmailAvailabilityQuery(string Email) : IRequest<Result<AvailabilityDto>>;
