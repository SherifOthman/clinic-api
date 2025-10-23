using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Clinics.Commands.UpdateClinic;

public record UpdateClinicCommand : IRequest<Result<ClinicDto>>
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
}
