using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Repositories;

public interface IStaffRepository : IRepository<Staff>
{
    Task<IEnumerable<Staff>> GetByClinicIdAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, Guid clinicId, CancellationToken cancellationToken = default);
    Task UpdateStatusAsync(Guid staffId, string status, Guid changedBy, string reason, CancellationToken cancellationToken = default);
    Task<IEnumerable<Staff>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
