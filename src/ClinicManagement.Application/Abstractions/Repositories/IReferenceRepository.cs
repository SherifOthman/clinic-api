using ClinicManagement.Application.Features.Reference.QueryModels;
using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Application.Abstractions.Repositories;

/// <summary>
/// Reference/lookup data: chronic diseases, specializations, subscription plans.
/// These are cached in memory — no need for full generic repo overhead.
/// </summary>
public interface IReferenceRepository
{
    // ── Reads (cached — public endpoints) ────────────────────────────────────
    Task<List<ChronicDiseaseRow>> GetChronicDiseasesAsync(CancellationToken ct = default);
    Task<List<SpecializationRow>> GetSpecializationsAsync(CancellationToken ct = default);
    Task<List<SubscriptionPlanRow>> GetActiveSubscriptionPlansAsync(CancellationToken ct = default);
    Task<bool> SpecializationExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> SubscriptionPlanExistsAsync(Guid id, CancellationToken ct = default);

    // ── Reads (paginated — admin endpoints, no cache) ─────────────────────────
    Task<(List<ChronicDisease> Items, int TotalCount)> GetChronicDiseasesPaginatedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<(List<Specialization> Items, int TotalCount)> GetSpecializationsPaginatedAsync(int pageNumber, int pageSize, CancellationToken ct = default);

    // ── Writes (invalidate cache after SaveChanges) ───────────────────────────
    Task<ChronicDisease?> GetChronicDiseaseByIdAsync(Guid id, CancellationToken ct = default);
    Task<Specialization?> GetSpecializationByIdAsync(Guid id, CancellationToken ct = default);
    Task<SubscriptionPlan?> GetSubscriptionPlanByIdAsync(Guid id, CancellationToken ct = default);

    void AddChronicDisease(ChronicDisease entity);
    void AddSpecialization(Specialization entity);
    void AddSubscriptionPlan(SubscriptionPlan entity);

    void InvalidateCache();
}
