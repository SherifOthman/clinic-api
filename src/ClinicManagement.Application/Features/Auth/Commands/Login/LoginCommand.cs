using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string EmailOrUsername,
    string Password,
    bool IsMobile
) : IRequest<LoginResult>;
