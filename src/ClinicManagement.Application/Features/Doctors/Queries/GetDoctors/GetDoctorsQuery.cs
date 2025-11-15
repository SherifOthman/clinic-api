using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Doctors.Queries.GetDoctors;

public record GetDoctorsQuery : IRequest<Result<List<DoctorDto>>>
{
    public int? SpecializationId { get; set; }
}
