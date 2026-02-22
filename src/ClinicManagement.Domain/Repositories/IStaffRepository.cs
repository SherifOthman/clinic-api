using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Repositories;

public interface IStaffRepository : IRepository<Staff>
{
    Task<IEnumerable<Staff>> GetByClinicIdAsync(int clinicId, CancellationToken cancellationToken = default);
}
