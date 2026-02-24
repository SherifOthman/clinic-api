using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Repositories;

public interface IClinicUsageMetricsRepository : IRepository<ClinicUsageMetrics>
{
    Task<ClinicUsageMetrics?> GetByClinicAndDateAsync(Guid clinicId, DateTime date, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClinicUsageMetrics>> GetMonthlyUsageAsync(Guid clinicId, DateTime monthStart, CancellationToken cancellationToken = default);
    Task UpsertAsync(ClinicUsageMetrics metrics, CancellationToken cancellationToken = default);
}
