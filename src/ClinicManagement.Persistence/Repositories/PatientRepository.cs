using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Patients.QueryModels;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using static ClinicManagement.Domain.Enums.BloodTypeExtensions;

namespace ClinicManagement.Persistence.Repositories;

public class PatientRepository : Repository<Patient>, IPatientRepository
{
    public PatientRepository(ApplicationDbContext context) : base(context) { }

    // ── Simple lookups ────────────────────────────────────────────────────────

    public async Task<Patient?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .Include(p => p.Phones)
            .Include(p => p.ChronicDiseases)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Patient?> GetDeletedByIdAsync(Guid id, CancellationToken ct = default)
        => await DbSet
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<bool> AnyByCodeAsync(string code, CancellationToken ct = default)
        => await DbSet
            .IgnoreQueryFilters()
            .AnyAsync(p => p.PatientCode == code, ct);

    public async Task<int> CountIgnoreFiltersAsync(CancellationToken ct = default)
        => await DbSet
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .CountAsync(ct);

    public async Task<int> CountCreatedFromAsync(DateTimeOffset from, CancellationToken ct = default)
        => await DbSet.CountAsync(p => p.CreatedAt >= from, ct);

    public async Task<int> CountCreatedBetweenAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
        => await DbSet.CountAsync(p => p.CreatedAt >= from && p.CreatedAt < to, ct);

    // ── Paginated list ────────────────────────────────────────────────────────

    public async Task<PaginatedResult<PatientListRow>> GetProjectedPageAsync(
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
        CancellationToken ct = default)
    {
        var query = isSuperAdmin
            ? DbSet.IgnoreQueryFilters([QueryFilterNames.Tenant]).AsNoTracking()
            : DbSet.AsNoTracking();

        // ── Filters ───────────────────────────────────────────────────────────

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

        if (countryGeonameId.HasValue)
            query = query.Where(p => p.CountryGeonameId == countryGeonameId.Value);

        if (stateGeonameId.HasValue)
            query = query.Where(p => p.StateGeonameId == stateGeonameId.Value);

        if (cityGeonameId.HasValue)
            query = query.Where(p => p.CityGeonameId == cityGeonameId.Value);

        if (isSuperAdmin && !string.IsNullOrWhiteSpace(clinicSearch))
        {
            if (Guid.TryParse(clinicSearch, out var clinicGuid))
                query = query.Where(p => p.ClinicId == clinicGuid);
            else
            {
                var matchingClinicIds = Context.Set<Clinic>()
                    .IgnoreQueryFilters([QueryFilterNames.Tenant])
                    .Where(c => c.Name.StartsWith(clinicSearch))
                    .Select(c => c.Id);
                query = query.Where(p => matchingClinicIds.Contains(p.ClinicId));
            }
        }

        // ── Sorting ───────────────────────────────────────────────────────────

        var desc = sortDirection.IsDescending();

        // When searching, rank by match quality instead of a sort column
        if (!string.IsNullOrWhiteSpace(searchTerm) && sortBy == null)
        {
            query = query
                .OrderByDescending(p => p.PatientCode == searchTerm)
                .ThenByDescending(p => p.FullName == searchTerm)
                .ThenByDescending(p => p.PatientCode.StartsWith(searchTerm))
                .ThenByDescending(p => p.FullName.StartsWith(searchTerm))
                .ThenByDescending(p => p.CreatedAt);
        }
        else
        {
            query = sortBy?.Trim().ToLower() switch
            {
                "fullname"            => desc ? query.OrderByDescending(p => p.FullName)              : query.OrderBy(p => p.FullName),
                "age"                 => desc ? query.OrderByDescending(p => p.DateOfBirth)           : query.OrderBy(p => p.DateOfBirth),
                "createdat"           => desc ? query.OrderByDescending(p => p.CreatedAt)             : query.OrderBy(p => p.CreatedAt),
                "chronicdiseasecount" => desc ? query.OrderByDescending(p => p.ChronicDiseases.Count) : query.OrderBy(p => p.ChronicDiseases.Count),
                _                     => query.OrderByDescending(p => p.CreatedAt),
            };
        }

        // ── Projection ────────────────────────────────────────────────────────
        // Only city name is shown in the table — country/state names not needed here

        var rawPage = await query
            .Select(p => new PatientListRaw(
                Id:                 p.Id.ToString(),
                PatientCode:        p.PatientCode,
                FullName:           p.FullName,
                DateOfBirth:        p.DateOfBirth,
                Gender:             p.Gender,
                BloodType:          p.BloodType,
                ChronicDiseaseCount: p.ChronicDiseases.Count,
                PrimaryPhone:       p.Phones.OrderBy(ph => ph.Id).Select(ph => ph.PhoneNumber).FirstOrDefault(),
                CreatedAt:          p.CreatedAt,
                ClinicId:           p.ClinicId,
                CountryGeonameId:   p.CountryGeonameId,
                StateGeonameId:     p.StateGeonameId,
                CityGeonameId:      p.CityGeonameId,
                CityNameEn:         p.City != null ? p.City.NameEn : null,
                CityNameAr:         p.City != null ? p.City.NameAr : null))
            .ToPagedAsync(pageNumber, pageSize, ct);

        // Clinic names are loaded separately — only for SuperAdmin
        var clinicNames = await LoadClinicNamesAsync(rawPage.Items.Select(p => p.ClinicId), isSuperAdmin, ct);

        var items = rawPage.Items.Select(p => new PatientListRow(
            Id:                 p.Id,
            PatientCode:        p.PatientCode,
            FullName:           p.FullName,
            DateOfBirth:        p.DateOfBirth,
            Gender:             p.Gender.ToString(),
            BloodType:          p.BloodType?.ToDisplayString(),
            ChronicDiseaseCount: p.ChronicDiseaseCount,
            PrimaryPhone:       p.PrimaryPhone,
            CreatedAt:          p.CreatedAt,
            ClinicId:           p.ClinicId,
            ClinicName:         clinicNames.GetValueOrDefault(p.ClinicId),
            CountryGeonameId:   p.CountryGeonameId,
            StateGeonameId:     p.StateGeonameId,
            CityGeonameId:      p.CityGeonameId,
            CityNameEn:         p.CityNameEn,
            CityNameAr:         p.CityNameAr)).ToList();

        return PaginatedResult<PatientListRow>.Create(items, rawPage.TotalCount, pageNumber, pageSize);
    }

    // ── Recent patients (dashboard) ───────────────────────────────────────────

    public async Task<List<RecentPatientRow>> GetRecentAsync(int count, CancellationToken ct = default)
        => await DbSet.AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .Select(p => new RecentPatientRow(
                p.Id.ToString(), p.PatientCode, p.FullName,
                p.DateOfBirth, p.Gender.ToString(), p.CreatedAt))
            .ToListAsync(ct);

    // ── Full detail ───────────────────────────────────────────────────────────

    public async Task<PatientDetailData?> GetDetailAsync(Guid id, bool isSuperAdmin, CancellationToken ct = default)
    {
        var baseQuery = isSuperAdmin
            ? DbSet.IgnoreQueryFilters([QueryFilterNames.Tenant]).AsNoTracking()
            : DbSet.AsNoTracking();

        var patient = await baseQuery
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id, p.PatientCode, p.FullName, p.DateOfBirth, p.Gender, p.BloodType,
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
        var clinicName = isSuperAdmin ? await LoadClinicNameAsync(patient.ClinicId, ct) : null;

        return new PatientDetailData(
            Id:             patient.Id,
            PatientCode:    patient.PatientCode,
            FullName:       patient.FullName,
            DateOfBirth:    patient.DateOfBirth,
            Gender:         patient.Gender.ToString(),
            BloodType:      patient.BloodType?.ToDisplayString(),
            CountryGeonameId: patient.CountryGeonameId,
            StateGeonameId:   patient.StateGeonameId,
            CityGeonameId:    patient.CityGeonameId,
            CountryNameEn:  patient.CountryNameEn,
            CountryNameAr:  patient.CountryNameAr,
            StateNameEn:    patient.StateNameEn,
            StateNameAr:    patient.StateNameAr,
            CityNameEn:     patient.CityNameEn,
            CityNameAr:     patient.CityNameAr,
            ClinicId:       patient.ClinicId,
            CreatedAt:      patient.CreatedAt,
            UpdatedAt:      patient.UpdatedAt,
            CreatedBy:      patient.CreatedBy,
            UpdatedBy:      patient.UpdatedBy,
            Phones:         patient.Phones,
            Diseases:       patient.Diseases,
            AuditUserNames: auditNames,
            ClinicName:     clinicName);
    }

    // ── Location filter options ───────────────────────────────────────────────

    public async Task<List<LocationOption>> GetLocationOptionsAsync(
        int? countryGeonameId, int? stateGeonameId, bool isSuperAdmin,
        CancellationToken ct = default)
    {
        var query = isSuperAdmin
            ? DbSet.IgnoreQueryFilters([QueryFilterNames.Tenant]).AsNoTracking()
            : DbSet.AsNoTracking();

        if (stateGeonameId.HasValue)
        {
            var rows = await query
                .Where(p => p.StateGeonameId == stateGeonameId.Value && p.CityGeonameId != null)
                .Select(p => new { p.CityGeonameId, p.City!.NameEn, p.City!.NameAr })
                .Distinct()
                .ToListAsync(ct);

            return rows
                .DistinctBy(r => r.CityGeonameId)
                .Select(r => new LocationOption(r.CityGeonameId!.Value, r.NameEn, r.NameAr))
                .OrderBy(o => o.NameEn)
                .ToList();
        }

        if (countryGeonameId.HasValue)
        {
            var rows = await query
                .Where(p => p.CountryGeonameId == countryGeonameId.Value && p.StateGeonameId != null)
                .Select(p => new { p.StateGeonameId, p.State!.NameEn, p.State!.NameAr })
                .Distinct()
                .ToListAsync(ct);

            return rows
                .DistinctBy(r => r.StateGeonameId)
                .Select(r => new LocationOption(r.StateGeonameId!.Value, r.NameEn, r.NameAr))
                .OrderBy(o => o.NameEn)
                .ToList();
        }

        var countryRows = await query
            .Where(p => p.CountryGeonameId != null)
            .Select(p => new { p.CountryGeonameId, p.Country!.NameEn, p.Country!.NameAr })
            .Distinct()
            .ToListAsync(ct);

        return countryRows
            .DistinctBy(r => r.CountryGeonameId)
            .Select(r => new LocationOption(r.CountryGeonameId!.Value, r.NameEn, r.NameAr))
            .OrderBy(o => o.NameEn)
            .ToList();
    }

    // ── Child entity helpers (use navigation properties via Context.Entry) ────

    public void AddPhone(PatientPhone phone)                  => Context.Set<PatientPhone>().Add(phone);
    public void RemovePhone(PatientPhone phone)               => Context.Set<PatientPhone>().Remove(phone);
    public void AddChronicDisease(PatientChronicDisease d)    => Context.Set<PatientChronicDisease>().Add(d);
    public void RemoveChronicDisease(PatientChronicDisease d) => Context.Set<PatientChronicDisease>().Remove(d);

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<Dictionary<Guid, string>> LoadClinicNamesAsync(
        IEnumerable<Guid> clinicIds, bool isSuperAdmin, CancellationToken ct)
    {
        if (!isSuperAdmin) return [];
        var ids = clinicIds.Distinct().ToList();
        if (ids.Count == 0) return [];

        return await Context.Set<Clinic>()
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .AsNoTracking()
            .Where(c => ids.Contains(c.Id))
            .Select(c => new { c.Id, c.Name })
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);
    }

    private async Task<string?> LoadClinicNameAsync(Guid clinicId, CancellationToken ct)
        => await Context.Set<Clinic>()
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .AsNoTracking()
            .Where(c => c.Id == clinicId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(ct);

    private async Task<Dictionary<Guid, string>> LoadAuditUserNamesAsync(
        IEnumerable<Guid?> userIds, CancellationToken ct)
    {
        var ids = userIds.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        if (ids.Count == 0) return [];

        return await Context.Set<User>()
            .AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, Name = (u.FirstName + " " + u.LastName).Trim() })
            .ToDictionaryAsync(u => u.Id, u => u.Name, ct);
    }

    // ── Private projection record ─────────────────────────────────────────────

    private record PatientListRaw(
        string Id, string PatientCode, string FullName, DateOnly DateOfBirth,
        Gender Gender, BloodType? BloodType, int ChronicDiseaseCount,
        string? PrimaryPhone, DateTimeOffset CreatedAt, Guid ClinicId,
        int? CountryGeonameId, int? StateGeonameId, int? CityGeonameId,
        string? CityNameEn, string? CityNameAr);
}
