using ClinicManagement.Application.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.Logout;

public class LogoutCommand : IRequest<Result>
{
    // No properties needed - uses CurrentUserService
}
