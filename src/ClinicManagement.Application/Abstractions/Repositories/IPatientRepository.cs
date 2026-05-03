using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Models.Filters;
using ClinicManagement.Application.Features.Patients.QueryModels;
using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

/// <summary>A single item in a location filter dropdown — both language names always returned.</summary>
public record LocationOption(int GeonameId, string NameEn, string NameAr);

public interface IPatientRepository : IRepository<Patient>
{
    Task<Patient?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<Patient?> GetDeletedByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> AnyByCodeAsync(string code, CancellationToken ct = default);
    Task<int> CountIgnoreFiltersAsync(CancellationToken ct = default);
    Task<int> CountCreatedFromAsync(DateTimeOffset from, CancellationToken ct = default);
    Task<int> CountCreatedBetweenAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);

    // ── Tenant-scoped (clinic users) ──────────────────────────────────────────

    Task<PaginatedResult<PatientListRow>> GetProjectedPageAsync(
        PatientFilter filter,
        string? nationalSearch,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);

    Task<List<RecentPatientRow>> GetRecentAsync(int count, CancellationToken ct = default);
    Task<PatientDetailData?> GetDetailAsync(Guid id, CancellationToken ct = default);

    Task<List<LocationOption>> GetLocationOptionsAsync(
        int? countryGeonameId, int? stateGeonameId,
        CancellationToken ct = default);

    // ── Cross-tenant (SuperAdmin only) ────────────────────────────────────────

    Task<PaginatedResult<PatientListRow>> GetAdminProjectedPageAsync(
        AdminPatientFilter filter,
        string? nationalSearch,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);

    Task<PatientDetailData?> GetAdminDetailAsync(Guid id, CancellationToken ct = default);

    Task<List<LocationOption>> GetAdminLocationOptionsAsync(
        int? countryGeonameId, int? stateGeonameId,
        CancellationToken ct = default);

    // ── Child entity helpers ──────────────────────────────────────────────────

    void AddPhone(PatientPhone phone);
    void RemovePhone(PatientPhone phone);
    void AddChronicDisease(PatientChronicDisease disease);
    void RemoveChronicDisease(PatientChronicDisease disease);
}
