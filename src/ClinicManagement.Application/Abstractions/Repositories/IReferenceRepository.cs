using ClinicManagement.Application.Features.Reference.QueryModels;

namespace ClinicManagement.Application.Abstractions.Repositories;

/// <summary>
/// Reference/lookup data: chronic diseases, specializations, subscription plans.
/// These are cached in memory — no need for full generic repo overhead.
/// </summary>
public interface IReferenceRepository
{
    Task<List<ChronicDiseaseRow>> GetChronicDiseasesAsync(CancellationToken ct = default);
    Task<List<SpecializationRow>> GetSpecializationsAsync(CancellationToken ct = default);
    Task<List<SubscriptionPlanRow>> GetActiveSubscriptionPlansAsync(CancellationToken ct = default);
    Task<bool> SpecializationExistsAsync(Guid id, CancellationToken ct = default);
    Task<bool> SubscriptionPlanExistsAsync(Guid id, CancellationToken ct = default);
}
