using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.UpdateProfile;

public record UpdateProfileCommand : IRequest<Result<UserDto>>
{
    public string FullName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
}
