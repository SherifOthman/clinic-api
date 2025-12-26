using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Queries.GetMe;

/// <summary>
/// Query to get the current authenticated user with auto-refresh capability.
/// Reads tokens from httpOnly cookies and handles token refresh automatically.
/// </summary>
public record GetMeQuery : IRequest<Result<UserDto>>
{
    // No properties needed - uses cookies from current HTTP context
}
