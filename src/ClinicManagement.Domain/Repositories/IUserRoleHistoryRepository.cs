using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Repositories;

public interface IUserRoleHistoryRepository : IRepository<UserRoleHistory>
{
    Task<IEnumerable<UserRoleHistory>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserRoleHistory>> GetByDateRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
}
