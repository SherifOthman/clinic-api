using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatientById;

public record GetPatientByIdQuery : IRequest<Result<PatientDto>>
{
    public int Id { get; set; }
}
