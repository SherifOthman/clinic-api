using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Auth.Commands.SwitchClinic;

public class SwitchClinicCommand : IRequest<Result<SwitchClinicResponse>>
{
    public Guid ClinicId { get; set; }
}