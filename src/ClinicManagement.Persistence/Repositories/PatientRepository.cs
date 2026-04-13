using ClinicManagement.Application.Abstractions.Repositories;
using ClinicManagement.Application.Common.Models;
using ClinicManagement.Application.Features.Patients.QueryModels;
using ClinicManagement.Application.Features.Patients.Queries;
using ClinicManagement.Domain.Common.Constants;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using static ClinicManagement.Domain.Enums.BloodTypeExtensions;

namespace ClinicManagement.Persistence.Repositories;

public class PatientRepository : Repository<Patient>, IPatientRepository
{
    private readonly DbSet<PatientPhone>          _phones;
    private readonly DbSet<PatientChronicDisease> _chronicDiseases;
    private readonly DbSet<Clinic>                _clinics;

    public PatientRepository(ApplicationDbContext context) : base(context)
    {
        _phones          = context.Set<PatientPhone>();
        _chronicDiseases = context.Set<PatientChronicDisease>();
        _clinics         = context.Set<Clinic>();
    }

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

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p =>
                p.FullName.StartsWith(searchTerm) ||
                p.PatientCode.StartsWith(searchTerm) ||
                p.Phones.Any(ph =>
                    ph.PhoneNumber.StartsWith(searchTerm) ||
                    (nationalSearch != null && ph.NationalNumber.StartsWith(nationalSearch))
                ));
        }

        if (!string.IsNullOrWhiteSpace(gender) &&
            Enum.TryParse<Gender>(gender, ignoreCase: true, out var genderEnum))
            query = query.Where(p => p.Gender == genderEnum);

        if (stateGeonameId.HasValue)
            query = query.Where(p => p.StateGeonameId == stateGeonameId.Value);

        if (cityGeonameId.HasValue)
            query = query.Where(p => p.CityGeonameId == cityGeonameId.Value);

        if (countryGeonameId.HasValue)
            query = query.Where(p => p.CountryGeonameId == countryGeonameId.Value);

        if (isSuperAdmin && !string.IsNullOrWhiteSpace(clinicSearch))
        {
            if (Guid.TryParse(clinicSearch, out var clinicGuid))
                query = query.Where(p => p.ClinicId == clinicGuid);
            else
            {
                var matchingIds = _clinics
                    .IgnoreQueryFilters([QueryFilterNames.Tenant])
                    .Where(c => c.Name.StartsWith(clinicSearch))
                    .Select(c => c.Id);
                query = query.Where(p => matchingIds.Contains(p.ClinicId));
            }
        }

        var desc = sortDirection.IsDescending();
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

        var rawPage = await query
            .Select(p => new PatientListRaw(
                p.Id.ToString(), p.PatientCode, p.FullName, p.DateOfBirth,
                p.Gender, p.BloodType, p.ChronicDiseases.Count,
                p.Phones.OrderBy(ph => ph.Id).Select(ph => ph.PhoneNumber).FirstOrDefault(),
                p.CreatedAt, p.ClinicId, p.CountryGeonameId, p.StateGeonameId, p.CityGeonameId))
            .ToPagedAsync(pageNumber, pageSize, ct);

        var clinicNames = await LoadClinicNames(rawPage.Items.Select(p => p.ClinicId), isSuperAdmin, ct);
        var items = rawPage.Items.Select(p => new PatientListRow(
            p.Id, p.PatientCode, p.FullName, p.DateOfBirth, p.Gender.ToString(),
            p.BloodType?.ToDisplayString(), p.ChronicDiseaseCount, p.PrimaryPhone,
            p.CreatedAt, p.ClinicId,
            clinicNames.TryGetValue(p.ClinicId, out var cn) ? cn : null,
            p.CountryGeonameId, p.StateGeonameId, p.CityGeonameId)).ToList();

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
            ? DbSet.IgnoreQueryFilters([QueryFilterNames.Tenant])
            : DbSet;

        var patient = await baseQuery.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id, p.PatientCode, p.FullName, p.DateOfBirth, p.Gender,
                p.BloodType,
                p.CountryGeonameId, p.StateGeonameId, p.CityGeonameId,
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

        var auditNames = await LoadAuditUserNames([patient.CreatedBy, patient.UpdatedBy], ct);
        var clinicName = isSuperAdmin ? await LoadClinicName(patient.ClinicId, ct) : null;

        return new PatientDetailData(
            patient.Id, patient.PatientCode, patient.FullName, patient.DateOfBirth,
            patient.Gender.ToString(), patient.BloodType?.ToDisplayString(),
            patient.CountryGeonameId, patient.StateGeonameId, patient.CityGeonameId,
            patient.ClinicId, patient.CreatedAt, patient.UpdatedAt,
            patient.CreatedBy, patient.UpdatedBy,
            patient.Phones, patient.Diseases, auditNames, clinicName);
    }

    // ── Distinct GeoNames IDs (for filter dropdowns) ──────────────────────────

    public async Task<List<int>> GetDistinctCountryGeonameIdsAsync(bool isSuperAdmin = false, CancellationToken ct = default)
    {
        var query = isSuperAdmin
            ? DbSet.IgnoreQueryFilters([QueryFilterNames.Tenant]).AsNoTracking()
            : DbSet.AsNoTracking();
        return await query
            .Where(p => p.CountryGeonameId != null)
            .Select(p => p.CountryGeonameId!.Value)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<List<int>> GetDistinctStateGeonameIdsAsync(bool isSuperAdmin = false, CancellationToken ct = default)
    {
        var query = isSuperAdmin
            ? DbSet.IgnoreQueryFilters([QueryFilterNames.Tenant]).AsNoTracking()
            : DbSet.AsNoTracking();
        return await query
            .Where(p => p.StateGeonameId != null)
            .Select(p => p.StateGeonameId!.Value)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<List<int>> GetDistinctCityGeonameIdsAsync(bool isSuperAdmin = false, CancellationToken ct = default)
    {
        var query = isSuperAdmin
            ? DbSet.IgnoreQueryFilters([QueryFilterNames.Tenant]).AsNoTracking()
            : DbSet.AsNoTracking();
        return await query
            .Where(p => p.CityGeonameId != null)
            .Select(p => p.CityGeonameId!.Value)
            .Distinct()
            .ToListAsync(ct);
    }

    // ── Child entity helpers ──────────────────────────────────────────────────

    public void AddPhone(PatientPhone phone)                  => _phones.Add(phone);
    public void RemovePhone(PatientPhone phone)               => _phones.Remove(phone);
    public void AddChronicDisease(PatientChronicDisease d)    => _chronicDiseases.Add(d);
    public void RemoveChronicDisease(PatientChronicDisease d) => _chronicDiseases.Remove(d);

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<Dictionary<Guid, string>> LoadClinicNames(
        IEnumerable<Guid> clinicIds, bool isSuperAdmin, CancellationToken ct)
    {
        if (!isSuperAdmin) return [];
        var ids = clinicIds.Distinct().ToList();
        if (ids.Count == 0) return [];
        return await _clinics
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .Where(c => ids.Contains(c.Id))
            .Select(c => new { c.Id, c.Name })
            .ToDictionaryAsync(c => c.Id, c => c.Name, ct);
    }

    private async Task<string?> LoadClinicName(Guid clinicId, CancellationToken ct)
        => await _clinics
            .IgnoreQueryFilters([QueryFilterNames.Tenant])
            .AsNoTracking()
            .Where(c => c.Id == clinicId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(ct);

    private async Task<Dictionary<Guid, string>> LoadAuditUserNames(
        IEnumerable<Guid?> userIds, CancellationToken ct)
    {
        var ids = userIds.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
        if (ids.Count == 0) return [];
        return await Context.Set<User>().AsNoTracking()
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, Name = (u.FirstName + " " + u.LastName).Trim() })
            .ToDictionaryAsync(u => u.Id, u => u.Name, ct);
    }

    private record PatientListRaw(
        string Id, string PatientCode, string FullName, DateOnly DateOfBirth,
        Gender Gender, BloodType? BloodType, int ChronicDiseaseCount,
        string? PrimaryPhone, DateTimeOffset CreatedAt, Guid ClinicId,
        int? CountryGeonameId, int? StateGeonameId, int? CityGeonameId);
}
