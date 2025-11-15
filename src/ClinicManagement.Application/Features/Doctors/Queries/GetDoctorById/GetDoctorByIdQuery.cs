using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Doctors.Queries.GetDoctorById;

public record GetDoctorByIdQuery : IRequest<Result<DoctorDto>>
{
    public int Id { get; set; }
}
