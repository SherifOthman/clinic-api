using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries.GetMe;

public record GetMeQuery(int UserId) : IRequest<GetMeDto?>;
