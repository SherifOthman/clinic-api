using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries.GetMe;

public record GetMeQuery : IRequest<Result<UserDto>>
{
    // No properties needed - uses cookies from current HTTP context
}
