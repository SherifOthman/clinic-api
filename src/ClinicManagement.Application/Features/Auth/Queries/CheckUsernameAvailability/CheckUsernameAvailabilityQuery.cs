using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries.CheckUsernameAvailability;

public record CheckUsernameAvailabilityQuery(
    string Username
) : IRequest<CheckUsernameAvailabilityDto>;
