using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.GoogleLogin;

/// <summary>
/// Processes a successful Google OAuth callback.
/// Finds or creates the user by email, then returns tokens.
/// </summary>
public record GoogleLoginCommand(
    string Email,
    string FullName,
    string? GoogleId,
    string? PictureUrl = null
) : IRequest<Result<TokenResponseDto>>;
