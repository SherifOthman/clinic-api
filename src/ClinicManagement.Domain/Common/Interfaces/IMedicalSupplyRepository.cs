using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IMedicalSupplyRepository : IRepository<MedicalSupply>
{
    Task<IEnumerable<MedicalSupply>> GetByClinicIdAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<MedicalSupply?> GetByIdAndClinicIdAsync(Guid id, Guid clinicId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MedicalSupply>> GetLowStockSuppliesAsync(Guid clinicId, int threshold = 10, CancellationToken cancellationToken = default);
}