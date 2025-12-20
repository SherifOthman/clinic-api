using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.SwitchClinic;

public record SwitchClinicCommand : IRequest<Result<AuthResponseDto>>
{
    public int UserId { get; set; }
    public int ClinicId { get; set; }
}
