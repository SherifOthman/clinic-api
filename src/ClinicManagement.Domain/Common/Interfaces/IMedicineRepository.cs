using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IMedicineRepository : IRepository<Medicine>
{
    Task<IEnumerable<Medicine>> GetByClinicIdAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<Medicine?> GetByIdAndClinicIdAsync(Guid id, Guid clinicId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Medicine>> GetLowStockMedicinesAsync(Guid clinicId, int threshold = 10, CancellationToken cancellationToken = default);
}