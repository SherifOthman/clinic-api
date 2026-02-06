using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ClinicManagement.Application.Features.Auth.Commands.UploadProfileImage;

public record UploadProfileImageCommand : IRequest<Result<UserDto>>
{
    public IFormFile File { get; init; } = null!;
}
