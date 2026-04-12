using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string EmailOrUsername,
    string Password,
    bool IsMobile
) : IRequest<Result<TokenResponseDto>>;
