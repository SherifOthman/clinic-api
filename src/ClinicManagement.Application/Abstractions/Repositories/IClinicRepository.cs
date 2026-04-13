using ClinicManagement.Application.Abstractions.Data;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

public interface IClinicRepository : IRepository<Clinic>
{
    Task<Clinic?> GetByOwnerIdAsync(Guid ownerUserId, CancellationToken ct = default);
    Task<bool> ExistsByOwnerIdAsync(Guid ownerUserId, CancellationToken ct = default);

    /// <summary>Batch-load clinic names by IDs (used by SuperAdmin views).</summary>
    Task<Dictionary<Guid, string>> GetNamesByIdsAsync(List<Guid> ids, CancellationToken ct = default);

    /// <summary>Find matching clinic IDs by name search (used by SuperAdmin filters).</summary>
    Task<List<Guid>> FindIdsByNameAsync(string nameSearch, CancellationToken ct = default);

    Task<int> CountIgnoreFiltersAsync(CancellationToken ct = default);

    /// <summary>Returns the clinic's ISO country code (e.g. "EG") for phone normalization.</summary>
    Task<string?> GetCountryCodeAsync(Guid clinicId, CancellationToken ct = default);
}
