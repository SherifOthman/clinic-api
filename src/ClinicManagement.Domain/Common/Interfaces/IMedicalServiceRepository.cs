using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IMedicalServiceRepository : IRepository<MedicalService>
{
    Task<IEnumerable<MedicalService>> GetByClinicBranchIdAsync(Guid clinicBranchId, CancellationToken cancellationToken = default);
}
