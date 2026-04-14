using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries;

/// <summary>
/// Returns distinct location options from actual patient data — both EN and AR names.
///   - No parent IDs        → countries that have at least one patient
///   - CountryGeonameId set → states in that country that have patients
///   - StateGeonameId set   → cities in that state that have patients
/// </summary>
public record GetPatientLocationOptionsQuery(
    int? CountryGeonameId,
    int? StateGeonameId,
    bool IsSuperAdmin
) : IRequest<Result<List<LocationOption>>>;
