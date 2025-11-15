using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Doctors.Commands.CreateDoctor;

public record CreateDoctorCommand : IRequest<Result<DoctorDto>>
{
    public int UserId { get; set; }
    public int SpecializationId { get; set; }
    public string? Bio { get; set; }
}
