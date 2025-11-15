using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IMedicineRepository : IRepository<Medicine>
{
    Task<IEnumerable<Medicine>> SearchMedicinesAsync(string? searchTerm = null, CancellationToken cancellationToken = default);
}
