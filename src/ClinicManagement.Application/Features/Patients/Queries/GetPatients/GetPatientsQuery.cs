using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.DTOs;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries.GetPatients;

public record GetPatientsQuery : IRequest<Result<List<PatientDto>>>;
