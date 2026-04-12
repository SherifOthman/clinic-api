using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries;

public record GetPatientCountriesQuery(bool IsSuperAdmin = false) : IRequest<Result<List<PatientStateDto>>>;
