using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.UpdateProfileImage;

public record UpdateProfileImageCommand : IRequest<Result<UserDto>>
{
    public string ProfileImageUrl { get; init; } = string.Empty;
}