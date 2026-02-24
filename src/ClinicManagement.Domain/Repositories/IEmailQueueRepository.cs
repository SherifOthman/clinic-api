using ClinicManagement.Domain.Entities;

namespace ClinicManagement.Domain.Repositories;

public interface IEmailQueueRepository : IRepository<EmailQueue>
{
    Task<IEnumerable<EmailQueue>> GetPendingEmailsAsync(int batchSize, CancellationToken cancellationToken = default);
}
