using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.DeleteProfileImage;

public record DeleteProfileImageCommand : IRequest<Result<UserDto>>;
