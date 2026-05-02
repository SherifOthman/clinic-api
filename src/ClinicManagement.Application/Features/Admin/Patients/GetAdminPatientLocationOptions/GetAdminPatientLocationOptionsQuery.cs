using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Admin.Patients;

/// <summary>
/// Cross-tenant location options — SuperAdmin only.
/// Returns distinct locations from ALL clinics' patient data.
/// </summary>
public record GetAdminPatientLocationOptionsQuery(
    int? CountryGeonameId,
    int? StateGeonameId
) : IRequest<Result<List<LocationOption>>>;
