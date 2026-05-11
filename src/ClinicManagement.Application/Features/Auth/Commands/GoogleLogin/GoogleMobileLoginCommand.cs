using ClinicManagement.Application.Common.Models;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.GoogleLogin;

/// <summary>
/// Processes a Google Sign-In from a mobile app.
/// The mobile app uses the Google Sign-In SDK to obtain an id_token,
/// then sends it here. We verify it with Google's tokeninfo endpoint,
/// extract the user's profile, and delegate to GoogleLoginHandler logic.
/// </summary>
public record GoogleMobileLoginCommand(string IdToken) : IRequest<Result<TokenResponseDto>>;
