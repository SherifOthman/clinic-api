using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Admin.Patients;

/// <summary>
/// Cross-tenant patient detail — SuperAdmin only.
/// Always includes ClinicId and ClinicName in the response.
/// </summary>
public record GetAdminPatientDetailQuery(Guid PatientId) : IRequest<Result<PatientDetailDto>>;
