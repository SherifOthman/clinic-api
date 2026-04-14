using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Patients.QueryModels;
using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IPatientRepository : IRepository<Patient>
{
    Task<Patient?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<Patient?> GetDeletedByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> AnyByCodeAsync(string code, CancellationToken ct = default);
    Task<int> CountIgnoreFiltersAsync(CancellationToken ct = default);

    Task<int> CountCreatedFromAsync(DateTimeOffset from, CancellationToken ct = default);
    Task<int> CountCreatedBetweenAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);

    Task<PaginatedResult<PatientListRow>> GetProjectedPageAsync(
        string? searchTerm,
        string? nationalSearch,
        string? gender,
        string? sortBy,
        string? sortDirection,
        string? clinicSearch,
        int? stateGeonameId,
        int? cityGeonameId,
        int? countryGeonameId,
        bool isSuperAdmin,
        int pageNumber,
        int pageSize,
        string lang,
        CancellationToken ct = default);

    Task<List<RecentPatientRow>> GetRecentAsync(int count, CancellationToken ct = default);
    Task<PatientDetailData?> GetDetailAsync(Guid id, bool isSuperAdmin, string lang, CancellationToken ct = default);

    /// <summary>Returns distinct GeoNames IDs of countries that have at least one patient.</summary>
    Task<List<int>> GetDistinctCountryGeonameIdsAsync(bool isSuperAdmin = false, CancellationToken ct = default);

    /// <summary>Returns distinct GeoNames IDs of states that have at least one patient.</summary>
    Task<List<int>> GetDistinctStateGeonameIdsAsync(bool isSuperAdmin = false, CancellationToken ct = default);

    /// <summary>Returns distinct GeoNames IDs of cities that have at least one patient.</summary>
    Task<List<int>> GetDistinctCityGeonameIdsAsync(bool isSuperAdmin = false, CancellationToken ct = default);

    void AddPhone(PatientPhone phone);
    void RemovePhone(PatientPhone phone);
    void AddChronicDisease(PatientChronicDisease disease);
    void RemoveChronicDisease(PatientChronicDisease disease);
}
