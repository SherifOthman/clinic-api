using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Domain.Common;
using MediatR;

namespace ClinicManagement.Application.Features.Patients.Queries;

/// <summary>
/// Returns distinct location options from actual patient data.
///
/// How it works:
///   - No parent IDs  → returns countries that have at least one patient
///   - CountryGeonameId set → returns states in that country that have patients
///   - StateGeonameId set  → returns cities in that state that have patients
///
/// Names are resolved from the seeded GeoNames tables (GeoCountries/GeoStates/GeoCities).
/// </summary>
public record GetPatientLocationOptionsQuery(
    int? CountryGeonameId,
    int? StateGeonameId,
    bool IsSuperAdmin,
    string Lang
) : IRequest<Result<List<LocationOption>>>;
