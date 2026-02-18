using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries.GetMe;

public record GetMeQuery(Guid UserId) : IRequest<GetMeDto?>;
