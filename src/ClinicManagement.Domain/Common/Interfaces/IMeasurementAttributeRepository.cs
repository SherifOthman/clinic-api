using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IMeasurementAttributeRepository : IRepository<MeasurementAttribute>
{
    Task<MeasurementAttribute?> GetByNameAsync(string nameEn, string? nameAr = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string nameEn, string? nameAr = null, CancellationToken cancellationToken = default);
}