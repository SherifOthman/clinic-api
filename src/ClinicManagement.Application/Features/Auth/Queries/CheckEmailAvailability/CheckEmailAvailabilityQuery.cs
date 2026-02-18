using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries.CheckEmailAvailability;

public record CheckEmailAvailabilityQuery(
    string Email
) : IRequest<CheckEmailAvailabilityDto>;
