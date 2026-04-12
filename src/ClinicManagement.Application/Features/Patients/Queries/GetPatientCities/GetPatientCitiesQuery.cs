using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries;

public record GetPatientCitiesQuery(bool IsSuperAdmin = false) : IRequest<Result<List<PatientStateDto>>>;
