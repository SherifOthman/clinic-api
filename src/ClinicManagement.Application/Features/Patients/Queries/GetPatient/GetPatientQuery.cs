using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatient;

public record GetPatientQuery : IRequest<Result<PatientDto>>
{
    public int Id { get; init; }
}
