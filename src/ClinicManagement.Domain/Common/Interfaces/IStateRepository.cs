using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Common.Interfaces;

public interface IStateRepository : IRepository<State>
{
    Task<State?> GetByGeonameIdAsync(int geonameId, CancellationToken cancellationToken = default);
}
