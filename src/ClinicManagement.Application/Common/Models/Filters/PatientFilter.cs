namespace ClinicManagement.Application.Common.Models.Filters;

/// <summary>
/// Filter + sort parameters for patient list queries.
/// Passed as a single object instead of 8+ individual parameters.
/// </summary>
public record PatientFilter(
    string?      SearchTerm       = null,
    string?      Gender           = null,
    int?         CountryGeonameId = null,
    int?         StateGeonameId   = null,
    int?         CityGeonameId    = null,
    string?      SortBy           = null,
    SortDirection SortDirection   = SortDirection.Asc
);

/// <summary>
/// Extends PatientFilter with cross-clinic search and deleted-record visibility — SuperAdmin only.
/// </summary>
public record AdminPatientFilter(
    string?      SearchTerm       = null,
    string?      Gender           = null,
    int?         CountryGeonameId = null,
    int?         StateGeonameId   = null,
    int?         CityGeonameId    = null,
    string?      SortBy           = null,
    SortDirection SortDirection   = SortDirection.Asc,
    string?      ClinicSearch     = null,
    bool         IncludeDeleted   = false
);
