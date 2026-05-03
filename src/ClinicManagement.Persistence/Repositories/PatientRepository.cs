using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Abstractions.Services;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Common.Models.Filters;
using ClinicManagement.Application.Features.Patients.QueryModels;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using ClinicManagement.Persistence.Security;
using Microsoft.EntityFrameworkCore;
using static ClinicManagement.Domain.Enums.BloodTypeExtensions;

namespace ClinicManagement.Persistence.Repositories;

public class PatientRepository : Repository<Patient>, IPatientRepository
{
    private readonly ICurrentUserService _currentUser;

    public PatientRepository(ApplicationDbContext context, ICurrentUserService currentUser)
        : base(context)
    {
        _currentUser = currentUser;
    }

    // ── Simple lookups ────────────────────────────────────────────────────────

    public async Task<Patient?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .Include(p => p.Phones)
            .Include(p => p.ChronicDiseases)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Patient?> GetDeletedByIdAsync(Guid id, CancellationToken ct = default)
        => await TenantGuard.AsUnfilteredQuery(DbSet).FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<bool> AnyByCodeAsync(string code, CancellationToken ct = default)
        => await TenantGuard.AsUnfilteredQuery(DbSet).AnyAsync(p => p.PatientCode == code, ct);

    public async Task<int> CountIgnoreFiltersAsync(CancellationToken ct = default)
        => await TenantGuard.AsSystemQuery(DbSet).CountAsync(ct);

    public async Task<int> CountCreatedFromAsync(DateTimeOffset from, CancellationToken ct = default)
        => await DbSet.CountAsync(p => p.CreatedAt >= from, ct);

    public async Task<int> CountCreatedBetweenAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
        => await DbSet.CountAsync(p => p.CreatedAt >= from && p.CreatedAt < to, ct);

    // ── Tenant-scoped list ────────────────────────────────────────────────────

    public async Task<PaginatedResult<PatientListRow>> GetProjectedPageAsync(
        PatientFilter filter,
        string? nationalSearch,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = DbSet.AsNoTracking();
        query = ApplyPatientFilters(query, filter.SearchTerm, nationalSearch, filter.Gender, filter.CountryGeonameId, filter.StateGeonameId, filter.CityGeonameId);
        query = ApplyPatientSort(query, filter.SearchTerm, filter.SortBy, filter.SortDirection);

        var rawPage = await ProjectPatientList(query, pageNumber, pageSize, ct);
        var items   = MapPatientListRows(rawPage.Items, clinicNames: []);
        return PaginatedResult<PatientListRow>.Create(items, rawPage.TotalCount, pageNumber, pageSize);    }

    // ── Tenant-scoped detail ──────────────────────────────────────────────────

    public async Task<PatientDetailData?> GetDetailAsync(Guid id, CancellationToken ct = default)
        => await FetchDetailAsync(DbSet.AsNoTracking(), id, includeClinicName: false, ct);

    // ── Tenant-scoped location options ────────────────────────────────────────

    public Task<List<LocationOption>> GetLocationOptionsAsync(
        int? countryGeonameId, int? stateGeonameId, CancellationToken ct = default)
        => FetchLocationOptionsAsync(DbSet.AsNoTracking(), countryGeonameId, stateGeonameId, ct);

    // ── Recent patients (dashboard) ───────────────────────────────────────────

    public async Task<List<RecentPatientRow>> GetRecentAsync(int count, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .Select(p => new RecentPatientRow(
                p.Id.ToString(), p.PatientCode,
                p.FullName, p.DateOfBirth, p.Gender.ToString(), p.CreatedAt))
            .ToListAsync(ct);

    // ── Admin (cross-tenant) list ─────────────────────────────────────────────

    public async Task<PaginatedResult<PatientListRow>> GetAdminProjectedPageAsync(
        AdminPatientFilter filter,
        string? nationalSearch,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        // IncludeDeleted = true → bypass both tenant AND soft-delete filters
        var query = filter.IncludeDeleted
            ? TenantGuard.AsUnfilteredQuery(DbSet).AsNoTracking()
            : TenantGuard.AsAdminQuery(DbSet, _currentUser).AsNoTracking();

        query = ApplyPatientFilters(query, filter.SearchTerm, nationalSearch, filter.Gender, filter.CountryGeonameId, filter.StateGeonameId, filter.CityGeonameId);

        if (!string.IsNullOrWhiteSpace(filter.ClinicSearch))
        {
            if (Guid.TryParse(filter.ClinicSearch, out var clinicGuid))
                query = query.Where(p => p.ClinicId == clinicGuid);
            else
            {
                var matchingClinicIds = TenantGuard.AsSystemQuery(Context.Set<Clinic>())
                    .Where(c => c.Name.StartsWith(filter.ClinicSearch))
                    .Select(c => c.Id);                query = query.Where(p => matchingClinicIds.Contains(p.ClinicId));
            }
        }

        query = ApplyPatientSort(query, filter.SearchTerm, filter.SortBy, filter.SortDirection);

        var rawPage    = await ProjectPatientList(query, pageNumber, pageSize, ct);
        var clinicNames = await LoadClinicNamesAsync(rawPage.Items.Select(p => p.ClinicId).ToList(), ct);
        var items       = MapPatientListRows(rawPage.Items, clinicNames);
        return PaginatedResult<PatientListRow>.Create(items, rawPage.TotalCount, pageNumber, pageSize);
    }

    // ── Admin (cross-tenant) detail ───────────────────────────────────────────

    public async Task<PatientDetailData?> GetAdminDetailAsync(Guid id, CancellationToken ct = default)
        => await FetchDetailAsync(
            TenantGuard.AsAdminQuery(DbSet, _currentUser).AsNoTracking(),
            id, includeClinicName: true, ct);

    // ── Admin (cross-tenant) location options ─────────────────────────────────

    public Task<List<LocationOption>> GetAdminLocationOptionsAsync(
        int? countryGeonameId, int? stateGeonameId, CancellationToken ct = default)
        => FetchLocationOptionsAsync(
            TenantGuard.AsAdminQuery(DbSet, _currentUser).AsNoTracking(),
            countryGeonameId, stateGeonameId, ct);

    // ── Child entity helpers ──────────────────────────────────────────────────

    public void AddPhone(PatientPhone phone)           => Context.Set<PatientPhone>().Add(phone);
    public void RemovePhone(PatientPhone phone)         => Context.Set<PatientPhone>().Remove(phone);
    public void AddChronicDisease(PatientChronicDisease d) => Context.Set<PatientChronicDisease>().Add(d);
    public void RemoveChronicDisease(PatientChronicDisease d) => Context.Set<PatientChronicDisease>().Remove(d);

    // ── Shared private helpers ────────────────────────────────────────────────

    private static IQueryable<Patient> ApplyPatientFilters(
        IQueryable<Patient> query,
        string? searchTerm, string? nationalSearch, string? gender,
        int? countryGeonameId, int? stateGeonameId, int? cityGeonameId)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(p =>
                p.FullName.StartsWith(searchTerm) ||
                p.PatientCode.StartsWith(searchTerm) ||
                p.Phones.Any(ph =>
                    ph.PhoneNumber.StartsWith(searchTerm) ||
                    (nationalSearch != null && ph.NationalNumber.StartsWith(nationalSearch))));

        if (!string.IsNullOrWhiteSpace(gender) &&
            Enum.TryParse<Gender>(gender, ignoreCase: true, out var genderEnum))
            query = query.Where(p => p.Gender == genderEnum);

        if (countryGeonameId.HasValue) query = query.Where(p => p.CountryGeonameId == countryGeonameId.Value);
        if (stateGeonameId.HasValue)   query = query.Where(p => p.StateGeonameId   == stateGeonameId.Value);
        if (cityGeonameId.HasValue)    query = query.Where(p => p.CityGeonameId    == cityGeonameId.Value);

        return query;
    }

    private static IQueryable<Patient> ApplyPatientSort(
        IQueryable<Patient> query, string? searchTerm, string? sortBy, string? sortDirection)
    {
        var desc = sortDirection.IsDescending();

        if (!string.IsNullOrWhiteSpace(searchTerm) && sortBy == null)
            return query
                .OrderByDescending(p => p.PatientCode == searchTerm)
                .ThenByDescending(p => p.FullName == searchTerm)
                .ThenByDescending(p => p.PatientCode.StartsWith(searchTerm))
                .ThenByDescending(p => p.FullName.StartsWith(searchTerm))
                .ThenByDescending(p => p.CreatedAt);

        return sortBy?.Trim().ToLower() switch
        {
            "fullname"           => desc ? query.OrderByDescending(p => p.FullName)              : query.OrderBy(p => p.FullName),
            "age"                => desc ? query.OrderByDescending(p => p.DateOfBirth)           : query.OrderBy(p => p.DateOfBirth),
            "createdat"          => desc ? query.OrderByDescending(p => p.CreatedAt)             : query.OrderBy(p => p.CreatedAt),
            "chronicdiseasecount"=> desc ? query.OrderByDescending(p => p.ChronicDiseases.Count) : query.OrderBy(p => p.ChronicDiseases.Count),
            _                    => query.OrderByDescending(p => p.CreatedAt),
        };
    }

    private static async Task<(List<PatientListRaw> Items, int TotalCount)> ProjectPatientList(
        IQueryable<Patient> query, int pageNumber, int pageSize, CancellationToken ct)
    {
        var rawPage = await query
            .Select(p => new PatientListRaw(
                Id: p.Id.ToString(),
                PatientCode: p.PatientCode,
                FullName: p.FullName,
                DateOfBirth: p.DateOfBirth,
                Gender: p.Gender,
                BloodType: p.BloodType,
                ChronicDiseaseCount: p.ChronicDiseases.Count,
                PrimaryPhone: p.Phones.OrderBy(ph => ph.Id).Select(ph => ph.PhoneNumber).FirstOrDefault(),
                CreatedAt: p.CreatedAt,
                ClinicId: p.ClinicId,
                CountryGeonameId: p.CountryGeonameId,
                StateGeonameId: p.StateGeonameId,
                CityGeonameId: p.CityGeonameId,
                CityNameEn: p.City != null ? p.City.NameEn : null,
                CityNameAr: p.City != null ? p.City.NameAr : null,
                IsDeleted: p.IsDeleted))
            .ToPagedAsync(pageNumber, pageSize, ct);

        return (rawPage.Items.ToList(), rawPage.TotalCount);
    }

    private static List<PatientListRow> MapPatientListRows(
        List<PatientListRaw> items, Dictionary<Guid, string> clinicNames)
        => items.Select(p => new PatientListRow(
            Id: p.Id,
            PatientCode: p.PatientCode,
            FullName: p.FullName,
            DateOfBirth: p.DateOfBirth,
            Gender: p.Gender.ToString(),
            BloodType: p.BloodType?.ToDisplayString(),
            ChronicDiseaseCount: p.ChronicDiseaseCount,
            PrimaryPhone: p.PrimaryPhone,
            CreatedAt: p.CreatedAt,
            ClinicId: p.ClinicId,
            ClinicName: clinicNames.GetValueOrDefault(p.ClinicId),
            CountryGeonameId: p.CountryGeonameId,
            StateGeonameId: p.StateGeonameId,
            CityGeonameId: p.CityGeonameId,
            CityNameEn: p.CityNameEn,
            CityNameAr: p.CityNameAr,
            IsDeleted: p.IsDeleted)).ToList();

    private async Task<PatientDetailData?> FetchDetailAsync(
        IQueryable<Patient> query, Guid id, bool includeClinicName, CancellationToken ct)
    {
        var patient = await query
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id, p.PatientCode, p.BloodType,
                FullName    = p.FullName,
                DateOfBirth = p.DateOfBirth,
                Gender      = p.Gender,
                p.CountryGeonameId, p.StateGeonameId, p.CityGeonameId,
                CountryNameEn = p.Country != null ? p.Country.NameEn : null,
                CountryNameAr = p.Country != null ? p.Country.NameAr : null,
                StateNameEn   = p.State   != null ? p.State.NameEn   : null,
                StateNameAr   = p.State   != null ? p.State.NameAr   : null,
                CityNameEn    = p.City    != null ? p.City.NameEn    : null,
                CityNameAr    = p.City    != null ? p.City.NameAr    : null,
                p.ClinicId, p.CreatedAt, p.UpdatedAt, p.CreatedBy, p.UpdatedBy,
                Phones   = p.Phones.Select(ph => ph.PhoneNumber).ToList(),
                Diseases = p.ChronicDiseases
                    .Select(cd => new PatientDiseaseRow(
                        cd.ChronicDiseaseId.ToString().ToLower(),
                        cd.ChronicDisease.NameEn,
                        cd.ChronicDisease.NameAr))
                    .ToList(),
            })
            .FirstOrDefaultAsync(ct);

        if (patient is null) return null;

        var auditNames = await LoadAuditUserNamesAsync([patient.CreatedBy, patient.UpdatedBy], ct);
        var clinicName = includeClinicName ? await LoadClinicNameAsync(patient.ClinicId, ct) : null;

        return new PatientDetailData(
            Id: patient.Id, PatientCode: patient.PatientCode,
            FullName: patient.FullName, DateOfBirth: patient.DateOfBirth,
            Gender: patient.Gender.ToString(), BloodType: patient.BloodType?.ToDisplayString(),
            CountryGeonameId: patient.CountryGeonameId, StateGeonameId: patient.StateGeonameId,
            CityGeonameId: patient.CityGeonameId,
            CountryNameEn: patient.CountryNameEn, CountryNameAr: patient.CountryNameAr,
            StateNameEn: patient.StateNameEn, StateNameAr: patient.StateNameAr,
            CityNameEn: patient.CityNameEn, CityNameAr: patient.CityNameAr,
            ClinicId: patient.ClinicId, CreatedAt: patient.CreatedAt,
            UpdatedAt: patient.UpdatedAt, CreatedBy: patient.CreatedBy, UpdatedBy: patient.UpdatedBy,
            Phones: patient.Phones, Diseases: patient.Diseases,
            AuditUserNames: auditNames, ClinicName: clinicName);
    }

    private static async Task<List<LocationOption>> FetchLocationOptionsAsync(
        IQueryable<Patient> query, int? countryGeonameId, int? stateGeonameId, CancellationToken ct)
    {
        if (stateGeonameId.HasValue)
        {
            var rows = await query
                .Where(p => p.StateGeonameId == stateGeonameId.Value && p.CityGeonameId != null)
                .Select(p => new { p.CityGeonameId, p.City!.NameEn, p.City!.NameAr })
                .Distinct().ToListAsync(ct);
            return rows.DistinctBy(r => r.CityGeonameId)
                .Select(r => new LocationOption(r.CityGeonameId!.Value, r.NameEn, r.NameAr))
                .OrderBy(o => o.NameEn).ToList();
        }

        if (countryGeonameId.HasValue)
        {
            var rows = await query
                .Where(p => p.CountryGeonameId == countryGeonameId.Value && p.StateGeonameId != null)
                .Select(p => new { p.StateGeonameId, p.State!.NameEn, p.State!.NameAr })
                .Distinct().ToListAsync(ct);
            return rows.DistinctBy(r => r.StateGeonameId)
                .Select(r => new LocationOption(r.StateGeonameId!.Value, r.NameEn, r.NameAr))
                .OrderBy(o => o.NameEn).ToList();
        }

        var countryRows = await query
            .Where(p => p.CountryGeonameId != null)
            .Select(p => new { p.CountryGeonameId, p.Country!.NameEn, p.Country!.NameAr })
            .Distinct().ToListAsync(ct);
        return countryRows.DistinctBy(r => r.CountryGeonameId)
            .Select(r => new LocationOption(r.CountryGeonameId!.Value, r.NameEn, r.NameAr))
            .OrderBy(o => o.NameEn).ToList();
    }

    private async Task<Dictionary<Guid, string>> LoadClinicNamesAsync(
        IEnumerable<Guid> clinicIds, CancellationToken ct)
    {
        var ids = clinicIds.Distinct().ToList();
        if (ids.Count == 0) return [];
        return await TenantGuard.AsSystemQuery(Context.Set<Clinic>()).AsNoTracking()
            .Where(c => ids.Contains(c.Id))
            .Select(c => new { c.Id, c.Name })
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);
    }

    private async Task<string?> LoadClinicNameAsync(Guid clinicId, CancellationToken ct)
        => await TenantGuard.AsSystemQuery(Context.Set<Clinic>()).AsNoTracking()
            .Where(c => c.Id == clinicId).Select(c => c.Name).FirstOrDefaultAsync(ct);

    private async Task<Dictionary<Guid, string>> LoadAuditUserNamesAsync(
        IEnumerable<Guid?> userIds, CancellationToken ct)
    {
        var ids = userIds.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        if (ids.Count == 0) return [];
        return await Context.Set<User>().AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, Name = u.FullName })
            .ToDictionaryAsync(u => u.Id, u => u.Name, ct);
    }

    private record PatientListRaw(
        string Id, string PatientCode, string FullName, DateOnly? DateOfBirth,
        Gender Gender, BloodType? BloodType, int ChronicDiseaseCount,
        string? PrimaryPhone, DateTimeOffset CreatedAt, Guid ClinicId,
        int? CountryGeonameId, int? StateGeonameId, int? CityGeonameId,
        string? CityNameEn, string? CityNameAr, bool IsDeleted);
}
