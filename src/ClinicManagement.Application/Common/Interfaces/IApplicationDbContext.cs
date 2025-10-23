using ClinicManagement.Domain.Common.Interfaces;

namespace ClinicManagement.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    IUnitOfWork UnitOfWork { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
