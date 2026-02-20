using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Repositories;

public interface IStaffRepository : IRepository<Staff>
{
    Task<Staff?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Staff>> GetByClinicIdAsync(int clinicId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Staff>> GetActiveByClinicIdAsync(int clinicId, CancellationToken cancellationToken = default);
}
