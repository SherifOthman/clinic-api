using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries;

public record GetPatientStatesQuery(bool IsSuperAdmin = false) : IRequest<Result<List<PatientStateDto>>>;

public record PatientStateDto(string NameEn, string NameAr);
