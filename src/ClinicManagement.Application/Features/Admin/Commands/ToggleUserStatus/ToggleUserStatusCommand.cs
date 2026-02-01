using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using ClinicManagement.Domain.Common.Models;
using MediatR;

namespace ClinicManagement.Application.Features.Admin.Commands.ToggleUserStatus;

public class ToggleUserStatusCommand : IRequest<Result<UserDto>>
{
    public Guid Id { get; set; }
}
